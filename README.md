# For Your Tomorrow — Systems Guide

How the game's code fits together: what each system does, which scripts make it up, and what you need to set up
in the Editor. All gameplay code lives in `Assets/Script`. Editor tools live in `Assets/Editor`.

- [Project layout](#project-layout)
- [How the systems talk to each other](#how-the-systems-talk-to-each-other)
- [Scenes, modes and input](#scenes-modes-and-input)
- [Player](#player)
- [Interaction](#interaction)
- [Items and inventory](#items-and-inventory)
- [Doors and locks](#doors-and-locks)
- [Hiding and vaulting](#hiding-and-vaulting)
- [Skills](#skills)
- [Sound and hearing](#sound-and-hearing)
- [Enemies](#enemies)
- [Getting caught and struggling](#getting-caught-and-struggling)
- [Checkpoints and respawning](#checkpoints-and-respawning)
- [Missions and objectives](#missions-and-objectives)
- [Dialogue](#dialogue)
- [Economy and shop](#economy-and-shop)
- [UI](#ui)
- [Tutorials and tips](#tutorials-and-tips)
- [Animation hooks](#animation-hooks)
- [Layers and rendering](#layers-and-rendering)
- [Debug tools](#debug-tools)
- [Known issues](#known-issues)

---

## Project layout

| Folder | Contents |
|---|---|
| `Animation/` | State machine behaviours and animation event receivers |
| `Checkpoint/` | Checkpoint triggers, save/load of player and level state |
| `Dialogue/` | Conversation data, flow, view and NPC trigger |
| `Enemy/` | Enemy controller, states, vision, movement, door navigation, routes |
| `Events/` | Static event buses (`PlayerEvents`, `UIEvents`, `WorldEvents`, `EnemyEvents`) |
| `Interactable/` | Interactables: doors, locks, items, hiding spots, NPCs, level objects |
| `Mission/` | Mission data, mission manager, mission board UI |
| `Objective/` | Objective chain and highlights |
| `Player/` | Movement, camera, animation, health, stamina, input, interaction, inventory, skills, struggle |
| `Scene/` | Scene names and scene loading |
| `Shop/` | Money and the sell shop |
| `SoundDetection/` | Noises enemies can hear |
| `Tutorial/` | Tutorial pop-ups |
| `UI/` | HUD composition root and its presenters |
| `Utility/` | Small shared helpers (`Cooldown`, `LayerHighlighter`) |
| Root | `MapManager`, `LouderFootstepArea`, `TipsTrigger` |

The `! VFX` and `! TechArtUpdate` folders hold the tech-art team's prototypes. They're separate from these systems
and aren't covered here.

---

## How the systems talk to each other

**Event buses.** Most systems never reference each other directly. They raise and listen to static events:

| Bus | Carries |
|---|---|
| `PlayerEvents` | Died, Respawned, OnCheckpoint, StruggleToggled, StruggleProgressChanged, SlowCrouch |
| `UIEvents` | ShowTip, read panel, password pad, mission board, shop board, dialogue requests, tutorial panel, tracking target |
| `WorldEvents` | LightsOutChanged, ItemCollected, ObjectiveShow, ObjectiveCompleted |
| `EnemyEvents` | Enemy indicator changes, confirm progress, AdvanceAllRoutesRequested |

Anything that subscribes in `OnEnable` unsubscribes in `OnDisable`.

**Singletons.** `MissionManager` and `EconomyManager` use `DontDestroyOnLoad`, so missions and money survive scene
changes. They must sit on root GameObjects. `CheckpointManager`, `MapManager` and `DialogueManager` live in one scene
each.

**Thin components.** The player and enemy are each split into small classes with one job, wired together by a
"composition root": `PlayerController` for the player body, `EnemyController` for the enemy, `UIManager` for the HUD.

---

## Scenes, modes and input

### Scenes

- `SceneConstants` holds the scene names: `Hideout`, `Mission1`.
- `SceneLoader.LoadScene` / `RestartScene` change scenes and reset the time scale.
- `HideoutMainDoor` loads the active mission's scene. `Mission1LeavePoint` returns to the hideout once the player
  carries the required item (the morse code).

### Base mode vs mission mode

`MapManager` (in the Hideout scene) picks the player's mode from the active scene:

| Scene | Input map | Animator controller |
|---|---|---|
| Hideout | `Player_Base` | `PlayerAnimController_Base` |
| Any other scene | `Player_Mission` | `PlayerAnimController_Mission` |

`MapManager` runs before other scripts (`[DefaultExecutionOrder(-100)]`), so anything asking for the mode in `Awake` or
`OnEnable` already gets the right answer.

### Input

- **`InputAction_Player`** has four maps:

  | Map | Actions |
  |---|---|
  | `Player_Mission` | Move, look, sprint, crouch, slow crouch, interact, skills, inventory |
  | `Player_Base` | Move, look, interact, inventory |
  | `UI` | UI navigation, inventory |
  | `Struggle` | Struggle |

- **`PlayerInputHandler`** turns the PlayerInput component's Unity Events into plain C# events (`OnMoveInput`,
  `OnCrouchPressed`, …). Keep the public `On…` method names; the Player prefab is wired to them.
- **`InputModeService`** decides the active map and the cursor:
  1. **Struggling** → `Struggle`
  2. **Any UI requested** (`RequestUI`) → `UI`, cursor free
  3. **Otherwise** → the scene's default map from `MapManager`

  Every panel calls `InputModeService.RequestUI(this, true/false)` when it opens and closes. The service keeps a set of
  requesters, so overlapping panels don't fight over the cursor.
- **`PlayerConstants`** holds the action map names. The PlayerInput component's **Default Map** must be
  `Player_Mission`; it's on the Player prefab.

---

## Player

### `PlayerController` (composition root)

Owns the movement state (`Normal`, `Crouching`, `Running`, `UsingSkill`, `Hiding`) and delegates the work:

| Helper | Job |
|---|---|
| `PlayerMotor` | Moves the `CharacterController`: gravity, horizontal movement, collider size per stance, teleporting |
| `PlayerCameraController` | Cinemachine third-person rig: follow height, shoulder offset, FOV for running / crouching / scanning, and a black vignette that fades in as stamina drops (**Tired Vignette Weight** at zero stamina) |
| `PlayerAnimation` | The only place that touches Animator parameters, and swaps Animator controllers |
| `PlayerStamina` | Running drains it, resting refills it, raises `Exhausted` / `Recovered` |
| `PlayerHealth` | Damage, heal, death and revive. `HealthScreenEffect` drives the blood vignette from the health ratio |

**Movement speeds:** walk, run (needs stamina), crouch, and **slow crouch**.

**Slow crouch** = crouching **and** holding the slow crouch key (Left Ctrl), in either order.
- The speed drops to **Slow Crouch Speed**.
- The crouch walk animation slows by the same ratio, through the `CrouchWalkSpeed` parameter, which is the crouch walk
  state's speed multiplier.
- Steps on a loud floor stay silent.

`PlayerController` also implements the interfaces other systems use to control the body:
- `IInteractorBody`: `SetCanMove`, `Teleport`, `SetPhysicsEnabled`
- `IHidingAgent`: entering and leaving hiding spots
- `ISkillUser`: dash, facing
- `IItemHolder`: the inventory

### Footsteps

- Animation events on the walk, run and crouch walk clips call `PlayerAnimationEventHandler`, which calls
  `PlayerSound`.
- Each step plays a footstep clip and makes a noise through `SoundEmitter`. Walking and running have their own noise
  radius.
- **Crouch steps are silent**, except on a loud floor (see [Sound and hearing](#sound-and-hearing)).
- Running out of stamina plays the tired sound.

---

## Interaction

`PlayerInteraction` runs a small pipeline every frame:

1. **`PlayerInteractionDetector` — what can I reach?** Tracks interactables inside the player's interaction trigger,
   then keeps only the ones the player can **see**:
   - A ray goes from **Ray Start** toward the centre of the target's solid colliders.
   - The ray stops at the first point where it touches the target.
   - Anything hit before that (other than the player) blocks the target. Each blocking object is logged once with its
     layer.
   - **Blocking Mask** on `PlayerInteraction` sets which layers can block.
   - In the Scene view, green lines are usable targets and red lines are blocked ones.
2. **`PlayerInteractionHandler` — what would each button do?**
   - **Items** go into a scrollable pick-up list, changed with the mouse wheel.
   - Everything else goes to the **nearest** usable interactable for its button (**F** or **Space**).
3. **`PlayerInteractionUI` — show it.** The pick-up list (`InteractItemUI`, `SelectionHandler`) and the world-space
   "[F]" prompt, drawn over interactables that implement `IPromptTarget`.

Interaction is blocked while running, using a skill or hiding. Pressing any interact button while hiding leaves the
hiding spot.

### Making something interactable

- Derive from **`Interactable`** to get the prompt anchor, or from `InteractableBase` for items. Override
  `OnInteract(GameObject interactor)`.
- Optional overrides: `InteractButton` (F by default), `CanInteract`, `Prompt`.
- Assign an `ObjectiveHighlight` to report the interaction to the [objective chain](#missions-and-objectives).

---

## Items and inventory

- **`ItemData`** (ScriptableObject):
  - name and icon
  - `maxStack`
  - `sellPrice` (0 = can't be sold)
- **`Item`** is the pick-up. Picking it up adds its `ItemData` to the player's `IItemHolder`, raises
  `WorldEvents.ItemCollected`, then calls `OnPickedUp`. By default the object is **hidden, not destroyed**, so a
  checkpoint respawn can put it back.

  | Item subclass | Adds |
  |---|---|
  | `MorseCode` | Restores power, advances every enemy route, completes Mission 1 |
  | `KeyForBasement` | Shows a hint and reveals the next objective |
  | `ReadableItem` / `ReadableItemData` | Opens the read panel with the item's text |

- **`InventoryModel`** holds the rules, with no UI code:
  - stacking, all-or-nothing `AddItem`, drag-and-drop `Move`, `Sort`
  - `Snapshot` / `Restore` for checkpoints
  - lives on `PlayerController.Inventory` (30 slots)
- **`Inventory`** is the grid view (Tab), built from `InventorySlot` views. It sits on the always-active PlayerCanvas,
  so it still hears the key while closed. It won't open over another UI or during a struggle, and it closes itself
  when the player is grabbed.

---

## Doors and locks

**`Door`** (an `Interactable`):
- **Using a door** teleports the player to the point on the other side (`PlayerPointFront` / `PlayerPointBack`) and
  toggles it open or closed. It plays its animation, turns its collider off briefly, and plays `DoorSound`.
- **Enemy pathing:** every door adds itself to `Door.ActiveDoors` for enemy route planning, and to
  `CheckpointManager` for saving. A closed door blocks enemy paths until an enemy opens it with `OpenForEnemy`.
  Once opened, the door's NavMeshLink stays on.
- **Double doors:** child `Door`s under a root `Door` count as one door for enemies (`Root`).

**Locks** are optional child components (`Lock`) that the door asks before opening:

| Lock | Rule |
|---|---|
| `NormalLock` | Opens from its **unlockable side**; from the other side only with **Required Key** if **Need Key** is on |
| `PasswordLock` | Opens the password pad (`PasswordInputUI`); only the lock knows the code |
| `ElectricLock` | Opens from its unlockable side; from the other side only while the lights are out |

---

## Hiding and vaulting

- **`Hidable`** (`Locker`, `Table`): interacting moves the player to **Enter Pos**, turns off collision and hides the
  model's renderers. Interacting again exits to **Exit Pos**. A spot holds one occupant.
- **`Vaultable`**: press **Space** to move over the obstacle along an arc, from side A to side B or back.

---

## Skills

**`SkillManager`** routes skill input:
- the equipped **physical skill** (hold the skill menu key for the radial menu, `SkillRadialMenu`)
- the **Demon Eye** scanner

| Skill | Behaviour |
|---|---|
| **Tackle** (`Tackle`, `TackleData`) | Locks onto the nearest stunnable enemy in range (shown by the tracking marker), dashes to it and stuns it. The player can't be caught mid-dash |
| **Demon Eye** (`ScannerSkill`) | Toggles a scan volume (`ScanTrigger`). Anything with `IScannable` inside it is revealed: `ScannableObject` and the current objective render on top of walls (`LayerHighlighter`). Turning it off starts its cooldown (`Cooldown`) |

Skills are built from `SkillData` assets, and each `Skill` handles its own press, release and tick.

---

## Sound and hearing

**`SoundEmitter.MakeSound(radius, reaction)`**:
1. Finds every collider on **Hearable Mask** within the radius. The buffer grows if it fills, so no listener is
   dropped.
2. For each `IHearable`, casts a line from the emitter to the listener. Solid colliders on the **Soundproof** layer
   block it; trigger colliders don't.
3. Listeners with a clear line receive a `SoundSignal`: position, radius, and reaction (`LookAt`, `WalkTo`, `RunTo`).

Set **Hearable Mask** to the Enemy layer. With Everything, every wall and chair in range is checked too.

**`LouderFootstepArea`** (trigger collider: Box, Sphere, Capsule or convex Mesh). While the player stands in it:
- footsteps use the area's clips
- the noise radius is multiplied by **Sound Multiplier**
- crouch steps make noise
- slow crouch keeps steps silent

The area is checked on each footstep, not with trigger enter/exit, so teleports (doors, hiding, respawn) can't leave
it stuck on.

---

## Enemies

### `EnemyController` (composition root)

Required components:

| Component | Job |
|---|---|
| `NavMeshAgent` | Movement |
| `EnemyVision` | Eyes |
| `EnemyMovement` | Base speed × named speed modifiers |
| `EnemyDoorNavigation` | Opens doors on the way |
| `EnemyAnimation` | Animator parameters |

The controller runs a state machine (`IEnemyState`, `EnemyStateBase`) and implements the interfaces other systems use:
`IHearable`, `IPlayerCatcher`, `IStunnable`, `IBumpable`.

### States

| State | What it does | Leaves to |
|---|---|---|
| `IdleState` | Walks to the idle point and faces its direction; may wait there (a scripted wait can't be interrupted) | Distracted (sound), Confirming (seen) |
| `PatrolState` | Walks the patrol points in order. At points with **Wait At This Point** it stops for Patrol Wait Time, facing the point's direction | Distracted, Confirming |
| `DistractedState` | `LookAt`: stops and turns to the sound. `WalkTo` / `RunTo`: goes there, then turns to it | Routine (after Distract Wait Time), Confirming |
| `ConfirmingState` | Stops and stares at what it glimpsed until the confirm timer fills. Breaking sight pauses the timer; re-spotting the player resumes it | Chase (confirmed), routine (out of sight for Confirm Give Up Time) |
| `ChaseState` | Plays **Found** standing still, then runs after the player | Catching (in reach), routine (lost them) |
| `CatchingState` | The grab and strangle sequence (see [Getting caught](#getting-caught-and-struggling)) | Stunned (player escaped), routine (player died) |
| `StunnedState` | Blind and frozen for Stun Time | Routine |

The **routine** is Patrol if the current route step has patrol points, otherwise Idle.

### Vision (`EnemyVision`)

The view cone is a wedge:
- **Width:** View Angle, checked horizontally only.
- **Reach:** View Radius, which grows while chasing.
- **Height:** View Vertical Size, a band centred on the eye. A player crouching right in front is still inside it.

Targets on **Target Mask** that aren't hidden behind **Obstacle Mask** are seen:
1. The first glimpse calls `OnSeenTarget`, which usually sends the enemy to Confirming.
2. After **Confirm Wait Time** of total sight the target is confirmed (`IsConfirmed`), and `ConfirmingState` moves to
   Chase. Losing sight pauses the timer during Confirming; in any other state it resets the timer.

`EnemyVisionEditor` draws the cone, its height band and the confirmed targets in the Scene view.

### Senses and reactions

- **Hearing:** noises reach the current state through `OnHearSound`.
- **Bumping:** when the player walks into an enemy, it's treated as a `LookAt` noise at the player's position.
  Patrol, Idle and Distracted react; Confirming, Chase, Catching and Stunned ignore it.

### Routes (`EnemyManager`)

- Each enemy gets a list of **steps** (`EnemyMovementStep`). A step is either Idle (an idle point) or Patrol (a list of
  `Waypoint`s), with a speed multiplier.
- An enemy moves to its next step when:
  - an Idle step's **To Next Step Wait Time** runs out (0 = never by itself), or
  - a level event calls `EnemyEvents.RequestAdvanceAllRoutes()`, for example the circuit board or the morse code.
- `EnemyStepTracking` applies the current step, pauses it while the enemy is busy, and resumes it afterwards.

### Opening doors (`EnemyDoorNavigation`, `DoorRoutePlanner`)

When a destination is behind closed doors:
1. `DoorRoutePlanner` runs a shortest-path search over door approach points to find the first door on the cheapest
   route.
2. The enemy walks to that door's approach point, opens it, and re-paths.
3. Several doors in a row are opened in route order.

States just call `SetDestination` and check `HasArrived`.

### Indicators

`EnemyEvents.IndicatorChanged` drives the icons that point toward enemies (`EnemyIndicatorPresenter`, `EnemyStateUI`,
`EnemyIndicatorUI`).
While an enemy is confirming, its icon steps through the **Confirming Level Sprites** on `UIManager` as the confirm
timer fills (a quarter of Confirm Wait Time each). If the player breaks line of sight, it holds its current level until
the player is seen again or the enemy gives up.

---

## Getting caught and struggling

**When it starts:** an enemy reaches the player in `ChaseState` and switches to `CatchingState`.

| Phase | Enemy | Player | Controls | Struggle |
|---|---|---|---|---|
| Grab | `StartCatch` | — | locked (Struggle map) | off |
| Hold | holds | Neck snapped into the enemy's **Right Hand**; Animator swapped to `PlayerAnimController_Caught` (`GotCaught`) | locked | off |
| Strangle | `CatchedPlayerLoop` (`Catching` = true) | `GotCaughtLoop` (`Caught` = true) | locked | **on**: presses count, the enemy deals damage |

**Phase changes come from the animations:**
- `StartCatchStateBehavior` reports when StartCatch ends.
- `GotCaughtStateBehavior` reports when GotCaught ends.
- If an animation never reports, each phase moves on after 5 seconds anyway, so the player can't be held forever.

**While held:**
- `PlayerStruggle` moves the whole player every `LateUpdate` so the neck stays in the hand.
- The player's `CharacterController` is off.
- The enemy's **Right Hand** is found automatically from the `hand.r` bone if left empty.

**How it ends:**

| Outcome | Player | Enemy |
|---|---|---|
| Player fills the struggle bar | Escapes | Stunned |
| Player's health runs out | Dies | Returns to its routine |

Either way, the player is let go and gets their Animator controller and stance back. The enemy plays `OutofCatch`, or
cross-fades to it if the catch ended before the loop.

---

## Checkpoints and respawning

**`CheckpointTrigger`** sends its **Spawn Point** through `PlayerEvents.OnCheckpoint` the first time the player
enters it. **`CheckpointManager`** then saves:

| Saved | Restored on respawn |
|---|---|
| Spawn point | Player teleported there |
| Inventory (slot by slot) | `InventoryModel.Restore` |
| Items picked up since the save | Shown again in the world |
| Mission statuses and active mission | `MissionManager.Restore` (statuses can go backwards) |
| Demon Eye cooldown | `SkillManager.RestoreScannerCooldown` |
| Money | `EconomyManager.RestoreMoney` |
| Lights out | `WorldEvents.SetLightsOut` |
| Every door (open, locked) | `Door.RestoreState` |
| Objective index | `ObjectiveManager.RestoreObjective` |
| Each enemy's route step | `EnemyManager.RestoreRouteSteps` |

**Respawn flow:**
1. The player dies and the game over screen appears (`GameOverPresenter`).
2. **Restart** calls `CheckpointManager.Load()`. With no checkpoint reached yet, the scene reloads instead.
3. Load restores everything above, revives the player and raises `PlayerEvents.Respawned`.
4. On `Respawned`, every enemy teleports to its routine spot (idle point or first patrol point) and restarts its
   routine.

`CheckpointManager` finds the player, `EnemyManager` and `ObjectiveManager` by itself if their fields are left empty.

---

## Missions and objectives

### Missions

- **`Mission`** is the data and the status rules: `Pending → OnProgress → Completed → Submitted`. Subclasses react in
  `OnAccepted` / `OnCompleted`.
- **`Mission1`**: get the morse code and return. It completes the moment the morse code is picked up.
- **`MissionManager`** (kept across scenes) holds the available missions and the active one.
- **`MissionNPC`** opens the mission board (`UIMissionNPC`, rows from `MissionButton`): list → details → accept.

### Objectives

- **`ObjectiveManager`** walks the level's `ObjectiveHighlight`s in order. Interacting with the current one (through
  the interactable it's assigned to) moves on to the next.
- The current objective can be revealed with `WorldEvents.RaiseObjectiveShow`. It's highlighted on top of walls while
  inside the Demon Eye scan.

### Mission 1 flow

1. Cut the power at the **circuit board**. Lights go out, electric locks open from both sides, and enemies move to
   their next route step.
2. Pick up the **morse code**. Power comes back, enemies advance again, and Mission 1 completes.
3. Leave through the **leave point** (needs the morse code), back to the hideout.

---

## Dialogue

- **`DialogueData`** (ScriptableObject) has two kinds of conversation:
  - A **sequence** of `DialogueLine`s, with no choices.
  - A **player line + NPC line + choices** (`DialogueChoice`). Each choice can show a response, chain to the next
    `DialogueData`, end the conversation, or **open the shop** (`openShopUI`).
- **`NPCDialogueInteract`** starts a conversation with `UIEvents.RequestDialogue`.
- **`DialogueManager`** runs the flow:
  - Clicking or pressing continue advances lines (`DialogueInput`).
  - Choices are buttons (`DialogueView`).
  - While open, the UI input map is active.

---

## Economy and shop

- **`EconomyManager`** holds the player's money (kept across scenes) and raises `MoneyChanged`.
- The shop opens from:
  - **`InteractShopNPC`**, or
  - a dialogue choice with **Open Shop UI** ticked.
- **`UISimpleSellShop`** (on the Player canvas) lists the inventory stacks with a sell price above 0. Each row is a
  `SellItemRow`. Selling a stack adds `sellPrice × amount` and empties the slot.

---

## UI

**`UIManager`** (on the Player prefab) holds the HUD references and creates one presenter (`IUIPresenter`) per panel.
Each presenter subscribes to its events while bound:

| Presenter | Shows |
|---|---|
| `ReadPanelPresenter` | Text of readable items |
| `TipPresenter` | Short hints that fade out (`FadeOutText`) |
| `PasswordPanelPresenter` | Password pad for a `PasswordLock` |
| `LightsOutPresenter` | Lights-out screen effect |
| `EnemyIndicatorPresenter` | Enemy state icons, on a circle around the screen centre |
| `GameOverPresenter` | Game over panel; Restart respawns at the last checkpoint |
| `TutorialPresenter` | Tutorial pages |

Other UI:
- **`PlayerStruggleUI`**: the "[Space] to free" prompt and progress bar.
- **`TrackingUIManager` / `UITargetTracking`**: an on-screen marker for the tackle target, clamped to the screen edge.
- **`EnemyIndicatorUI`**: places each enemy's icon on a circle around the screen centre (**Radius**), in the enemy's
  direction. Icons stay on the circle even when the enemy is on screen.
- **Mission board**, **inventory**, **shop**: see their sections.

---

## Tutorials and tips

- **`TutorialTrigger`** opens the tutorial panel with its `TutorialData` (header, body, image) the first time the
  player enters.
- **`TipsTrigger`** shows a one-time hint message.

---

## Animation hooks

| Script | Where it goes | Does |
|---|---|---|
| `InteractStateBehavior` | Player pick-up states | Freezes movement while the interact animation plays |
| `FoundStateBehavior` | Enemy `Found` state | Tells `ChaseState` the Found reaction is over |
| `StartCatchStateBehavior` | Enemy `StartCatch` state | Tells `CatchingState` the grab reached the player |
| `GotCaughtStateBehavior` | Player `Arthur_CaughtStart` (Caught controller) | Tells `PlayerStruggle` GotCaught is over |
| `AnimationEndStateBehavior` | (base class) | Reports once when a state's clip reaches its end |
| `PlayerAnimationEventHandler` | Player model (animation events) | `PlayWalkSound`, `PlayRunSound`, `PlayCrouchWalkSound` |

Animator parameters used from code:

| Animator | Parameters |
|---|---|
| Player (base / mission) | `Move` (0 idle, 1 walk, 2 run), `Crouch`, `Interact`, `CrouchWalkSpeed` |
| Player (Caught controller) | `Caught` |
| Enemy | `Move`, `Found`, `StartCatch`, `Catching` |

---

## Layers and rendering

| Layer | Used for |
|---|---|
| Player | The player; what enemies look for (Target Mask) |
| RenderOnTop | Player X-ray silhouette through walls |
| ObjectiveHighlight | Highlighted objectives and scanned objects |
| Enemy | Enemies; what sounds reach (Hearable Mask) and what Tackle targets |
| Soundproof | Solid objects that block sound |

`PC_Renderer` draws RenderOnTop and ObjectiveHighlight with its **Render Objects** features (X-ray, highlight). Its
Opaque, Transparent and Depth Prepass layer masks are set to **everything except those two**. A new layer renders
normally without extra setup.

---

## Debug tools

| Tool | Where |
|---|---|
| Take 10 damage / Heal 10 | `PlayerHealth` context menu |
| Make Sound | `SoundEmitter` context menu (uses Debug Radius and Cur Reaction) |
| Next Objective | `ObjectiveManager` context menu |
| Sound line checks | Scene view: green reached, red blocked by Soundproof |
| Interaction line checks | Scene view: green usable, red blocked; Console logs the blocking objects |
| Vision cone | Scene view, with the enemy selected (`EnemyVisionEditor`) |

---

## Known issues

- **`PlayerAnimation` breaks player builds.** Its base and mission controller fields use `UnityEditor.Animations`,
  which only exists in the editor. Change them to `RuntimeAnimatorController` before building.
- **The inventory doesn't carry over between scenes.** It belongs to the player in each scene, so items picked up in a
  mission are gone when the hideout loads, and there's nothing to sell yet.
- **Lights-out tracking assumes each scene starts with the lights on.**
