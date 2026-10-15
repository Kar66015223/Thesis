# SOLID Refactor – Review & Advice

Branch: `refactor/solid` (uncommitted). Scope: every script under `Assets/Script`, plus `Assets/Editor` and a light touch on `Assets/! VFX`. TextMesh Pro samples and the TutorialInfo package were left alone.

## How the refactor was done (and what it guarantees)

- **No scene or prefab was edited.** Every MonoBehaviour keeps its class name and file, so no script references break.
- **Inspector data is preserved.** Serialized field names were kept, or renamed with `[FormerlySerializedAs]` (`ObjectiveManager`). I checked every YAML key stored in your prefabs and scenes against the new code. The only keys no longer read are runtime/debug flags (`hasInteracted`, `isCooldown`, `takeDamage`, `makeSound`, `next`…), the test flags `hasKey`/`hasMorseCode`, and a few keys the old code already ignored (`interactPromptText`, `m1Sprite`, `selectedSize`).
- **Wiring by name is untouched.** The `PlayerInputHandler.On…(CallbackContext)` methods (wired to *PlayerInput → Invoke Unity Events*) and the `PlayWalkSound`/`PlayRunSound` animation events keep their names.
- **Compiles with 0 errors and 0 warnings** against the real Unity 6000.3 / package assemblies, for both the game and Editor assemblies. Unity (already open) also recompiled and reimported the prefabs with no errors.
- **Not play-tested.** I can't drive Play Mode from here. Use the checklist at the bottom before you merge.

---

## Bugs found and fixed

| # | Severity | Bug | Cause |
|---|---|---|---|
| 1 | **High** | Items (key, morse code, readable note) could never be picked up | Since commit `6c12adb`, `PlayerInteractionUI` ran a second pass that overwrote the selected item with the first interactable, or with `null` |
| 2 | **High** | Player frozen after opening the password door | `Door` called `SetCanMove(false)`, and only the Interact animation turns movement back on. The password path plays no animation |
| 3 | **High** | Tackle could freeze the player forever | If the enemy left range mid-dash, `Tick()` returned before the dash code: `isDashing` stayed true and `canMove` stayed false |
| 4 | Medium | "Mission1 completed" ran every frame after pickup, and threw a NullReferenceException in scenes without a `Player` tag | `MissionManager.Update` → `CheckCompletion()` → `FindGameObjectWithTag` → `Complete()` every frame |
| 5 | Medium | Enemy "?" icon never filled up | Icons were keyed by `stateUIPoint` but confirm progress was keyed by the enemy's `transform`, so the lookup always missed |
| 6 | Medium | "?" icon stayed on after the enemy gave up | `ConfirmingState.Exit` never cleared it, because each state had to remember to clear its own icon |
| 7 | Medium | Crouch / Demon-Eye toggles got out of sync (pressing crouch while running, then needing 2–3 presses) | `PlayerInputHandler` flipped the toggle even when the controller refused the request |
| 8 | Medium | Player kept running after uncrouching without holding Shift | Releasing Shift while crouched was ignored, so the run intent stayed `true` |
| 9 | Medium | `ElectricLock` ignored objective resets | Its `OnEnable` hid `Interactable.OnEnable`, and Unity only calls the most-derived message |
| 10 | Medium | Two open windows fought over the cursor (closing one locked it while the other was still open) | Four classes each set `Cursor.lockState` and the action map themselves |
| 11 | Low | Old tackle tracking marker left behind when switching targets; a "clear" event fired every frame | `TrackingUIManager` only cleared everything on `null` |
| 12 | Low | `[F]` prompt drawn over a different object than the one F would use | The prompt showed on the *last* detected object, while F used the *first* |
| 13 | Low | Player walked by itself after a dialogue ended | `moveInput` kept its last value while `PlayerController` was disabled |
| 14 | Low | Character collider had height 0 on the first frame | `Awake` used `currentColliderY/Height` before they were set |
| 15 | Low | `Door.TurnOffCollider` threw a NullReferenceException with no collider, and overlapping coroutines re-enabled the collider early | `yield return null` was used instead of `yield break`, and earlier coroutines were never stopped |
| 16 | Low | `Die()` could fire many times; healing from low to half HP kept the low-HP breathing | No `IsDead` guard; the half-HP branch never set the breath frequency |
| 17 | Low | Enemy vision raycast length didn't match its direction | Distance was measured feet-to-feet, but the ray went from the eye to the head |
| 18 | Low | `DialogueManager.cs` / `NPCDialogueInteract.cs` comments showed as garbage | The files were saved as Windows-874; they are now UTF-8 and the Thai comments are readable again |
| 19 | Low | Console spam | Logs fired every second per door, every frame in `ChaseState`, and on every item interaction |

### Intentional behaviour changes

- **Player state priority is now Hiding > UsingSkill > Running > Crouching > Normal.** Before, Crouching won, so you could be "Crouching" while hiding or while using the skill (and could interact during the skill).
- **Who F/Space targets:** an item selected in the list wins. Otherwise the **nearest** usable object wins (before, it was whichever was detected first).
- The Interact animation only plays when something was actually used.
- **Picked-up items now go into the player's inventory model.** `KeyForBasement` and `MorseCode` no longer set flags on `PlayerController`.
- **An objective completes once**, even if several interactables point to the same `ObjectiveHighlight`.
- **Read, Password, Mission board, Dialogue and Game Over switch to the UI action map.** You can't vault or interact behind an open window.
- Crouch is ignored while hiding.
- Mouse-wheel item scrolling now goes through the Input System instead of the legacy `Input` class.

### Deliberately *not* changed

- **A door's NavMeshLink stays enabled after the first time it opens** (same as before). So enemies walk through a door the player closes again. I kept this because disabling a link under an agent that is crossing it can get the agent stuck. If you want closed doors to block enemies again, disable the link on close and make sure no agent is on it.
- **`TackleData.cooldown = 10` and `cost = 30` are still not enforced.** Turning them on would change game balance. A `Cooldown` helper now exists (`Utility/Cooldown.cs`), so enabling it is a few lines in `Tackle`.

---

## SOLID cheat-sheet (how each principle showed up in your code)

| Principle | What it means in Unity terms | Where it was violated | What I did |
|---|---|---|---|
| **S** – Single Responsibility | One reason to change per class | `PlayerController` handled movement, state, interaction routing, Demon Eye, hiding and camera feeding. `UIManager` ran 7 unrelated panels. `DialogueManager` mixed flow, UI, input and cursor | Split into `PlayerMotor`, `PlayerInteraction`, `SkillManager`, `InputModeService`, the UI presenters, `DialogueView` and `DialogueInput` |
| **O** – Open/Closed | Add features by adding classes, not editing old ones | `ScanTrigger` checked each concrete type. `SkillManager` switched on `SkillUsage`. UI and Vision checked `CurrentState is ChaseState` | `IScannable`; skills handle their own input; states expose `Indicator`; locks are strategies |
| **L** – Liskov Substitution | A subclass must work anywhere its base works | `Mission.OnSubmit` threw `NotImplementedException`. `Lock` *was* an `Interactable` but could never be used directly. `Item` and `Interactable` duplicated each other | Missions have guarded virtual hooks; `Lock` is now a plain strategy; `InteractableBase` holds the shared logic |
| **I** – Interface Segregation | Don't force classes to implement what they don't use | `Skill` forced `Use`/`HoldUse`/`HandleUse` with empty bodies. States repeated empty `OnHearSound`/`OnSeenTarget`. `IInteractable` exposed setters | Virtual no-op defaults (`EnemyStateBase`, `Skill`); read-only `IInteractable`; small capability interfaces |
| **D** – Dependency Inversion | Depend on abstractions, not concrete classes | Interactables, locks, items and missions reached into `PlayerController`. `Tackle` and `PlayerStruggle` did `new StunnedState(enemy)` | `IInteractorBody`, `IHidingAgent`, `IItemHolder`, `ISkillUser`, `IStunnable`, `IPlayerCatcher`, `IPasswordLock`, `IFlashable` |

> **A pragmatic note for a thesis project:** SOLID in Unity is about *small components plus data in ScriptableObjects*, not about putting an interface on every class. I only added an interface where a second implementation already exists or is clearly coming (enemies vs player, several scannable or lock types). The rest uses plain classes.

---

## System by system

### 1. Event bus (`GameEvent` → `Events/UIEvents`, `PlayerEvents`, `EnemyEvents`, `WorldEvents`)

**What you did right**
- You used the Observer pattern so gameplay doesn't hold direct references to UI.
- You subscribed and unsubscribed consistently in `OnEnable`/`OnDisable`, so there were no leaked handlers.

**What was wrong**
- The events were `public static Action` fields. Anyone could *invoke* them, or wipe every subscriber with `GameEvent.X = null`.
- One class mixed UI, enemy and objective events. Action maps were passed around as magic strings.

**What changed**
- Real C# `event`s that can only be raised through methods (`UIEvents.ShowTip(...)`).
- Split into four channels, so each subscriber depends only on the channel it needs (Interface Segregation).
- `OnChangeEnemyPath` was 1-to-1 and is now a plain C# event on `EnemyStepTracking`.

**Best approach next:** **ScriptableObject event channels** (the "Unite Austin 2017 – Game Architecture with ScriptableObjects" approach). Designers can wire them in the Inspector, there are no statics, and each scene/prefab is easy to test on its own.

### 2. Input (`PlayerInputHandler`, `InputModeService`)

**Right**
- `PlayerInputHandler` was already a clean adapter from the Input System to C# events, which is textbook decoupling.

**Wrong**
- Toggle state lived in the adapter (bug #7).
- Cursor and action-map switching was copy-pasted in 4 places (bug #10).
- Legacy `Input` was mixed in (scroll and dialogue).

**Changed**
- The adapter now only reports presses.
- `InputModeService` is the single owner of the cursor and the action map. Windows *request* UI mode and *release* it, and destroyed requesters are pruned automatically.

**Best approach**
- Generate the C# class from `InputSystem_Actions.inputactions` and implement its `IPlayerActions` interface. This is type-safe and avoids wiring methods by name in the Inspector.
- Add a "Scroll" and a "Submit" action so dialogue and item scrolling also go through action maps.

### 3. Player (`PlayerController`, `PlayerMotor`, `PlayerCameraController`, `PlayerAnimation`)

**Right**
- You kept the camera and animation as plain `[Serializable]` classes (composition, not inheritance).
- You cached `Animator.StringToHash`.
- You used `RequireComponent`.

**Wrong**
- `PlayerController` was a god class.
- Other code wrote into the camera's public fields (`camController.isCrouching = …`, `camForward = …`).
- Test flags lived on the controller.
- `stamina` null checks were inconsistent.

**Changed**
- `PlayerController` is now a composition root: it reads movement input, owns the movement state, and delegates to `PlayerMotor` (gravity, move, stance, teleport).
- The camera receives a `CameraViewState` each frame.
- Interaction moved to `PlayerInteraction`, and Demon Eye moved to `SkillManager`.
- The controller implements small capability interfaces that other systems use instead of the concrete class.

**Best approach**
- When the editor is available, split the remaining pieces into **components** (`PlayerMovement`, `PlayerCrouch`, `PlayerHiding`, `PlayerInventory`) on the Player prefab.
- If the player gets more states (climb, carry, ledge…), reuse the State pattern you already wrote for enemies.

### 4. Stamina / Health / Sound / Struggle

**Right**
- Clear tuning fields with `Range` and headers.
- `PlayerSound` avoided repeating the same footstep clip.
- `PlayerStruggleUI` was already fully event-driven.

**Wrong**
- Stamina called `PlayerSound` directly and exposed public mutable bools.
- Health drove the UI, the controller and a material all at once, and used debug bools polled in `Update`.
- Struggle created enemy states and was ticked by the enemy.

**Changed**
- Stamina exposes `SetMoving`/`SetRunIntent` and raises `Exhausted`/`Recovered`; `PlayerSound` listens.
- Health raises `HealthChanged`/`Died`; `HealthScreenEffect` (its own class) renders the effect.
- Debug actions are now `[ContextMenu]` entries.
- `PlayerStruggle` runs its own loop and talks to the enemy through `IPlayerCatcher.ReleasePlayer(escaped)`. The enemy decides whether that means stun or patrol.

**Best approach**
- Move the blood-effect fields into their own `PlayerHealthEffects` component. I kept them on `PlayerHealth` only so your Inspector values survive.
- `bloodFX` is a shared material asset. Prefer a runtime material instance or a Volume parameter, so Play Mode never changes the asset on disk.

### 5. Interaction (`PlayerInteraction`, detector / handler / UI)

**Right**
- Splitting into detector, handler and UI was the correct idea.
- The overlap re-check (`ComputePenetration`) correctly handles colliders that get disabled.

**Wrong**
- The UI chose the selection (bug #1).
- The detector called the UI directly.
- The prompt was drawn by the interactable (`Interactable.TogglePrompt`), which is UI code inside a domain object.
- Two parallel lists had to be kept in sync.
- LINQ allocated on every key press, and `GetComponentsInChildren` ran every frame.

**Changed**
- Detector = what overlaps. Handler = what each button targets. UI = draw only.
- Interactables don't know about the player's state; the player decides whether it may interact.

**Best approach**
- Let interactables provide their own prompt text (`"Open"`, `"Hide"`, `"Read"`) through `IPromptTarget`.
- Add an outline highlight on the current target.

### 6. Interactables, Doors and Locks

**Right**
- A common base class and interface.
- Door positioning points.
- `TryGetReachableApproach` for enemies is solid.

**Wrong**
- Every subclass had to remember `base.Interact()` and a `CanInteract` re-check (Hidable and Vaultable checked twice).
- `Lock` inherited from `Interactable` and recursed `Door → Lock → Door`.
- `NormalLock` was a tangle of four boolean branches.
- The password check exposed the code (`GetCorrectPassword()`).

**Changed**
- `InteractableBase.Interact` is a **Template Method**: availability check → objective report → `OnInteract`.
- `Lock` is a **Strategy** (`TryUnlock` returns whether the door may open now).
- `IPasswordLock.TrySubmit` keeps the code inside the lock.
- Doors register themselves (`Door.ActiveDoors`), so enemies don't call `FindObjectsByType` every second.

**Best approach**
- For one-off level scripts (`CircuitBoard`, `MissionNPC`, `HideoutMainDoor`, `Mission1LeavePoint`), use **one** `Interactable` with a list of reusable *action* components or a `UnityEvent onInteract`. Then a new level object needs no new C# class (Open/Closed).
- Make lock conditions data-driven too (`HasItemCondition`, `PowerOffCondition`, `SideCondition`).

### 7. Items & Inventory (`Item`, `InventoryModel`, `Inventory`, `InventorySlot`)

**Right**
- `ItemData` as a ScriptableObject.
- The inventory's all-or-nothing `AddItem` and stacking rules were well thought out.

**Wrong**
- Picking up a key set `ctrl.hasKey` (the "delete later" flag).
- The inventory mixed rules with UI and drag/drop, used an `EMPTY_ITEM` sentinel, and wasn't connected to anything.
- `SortItem`'s ascending/descending ternary used the same comparer in both branches.

**Changed**
- `InventoryModel` is pure rules and implements `IItemHolder`. `Inventory`/`InventorySlot` are views over it.
- Picking up an item stores its `ItemData`.
- `NormalLock.requiredKey` and `Mission1LeavePoint.requiredItem` check the holder.

**Best approach**
- Add a `PlayerInventory` component that owns the model (it lives on `PlayerController` for now; one-line move).
- Assign the two ItemData fields in the Inspector. Until you do, they find the scene's `KeyForBasement` / `MorseCode` and log a warning.

### 8. Objectives

**Right**
- Using the URP render-layer swap to highlight objectives.

**Wrong**
- Every interactable/item stored its own `hasInteracted` copy of objective state.
- The highlight code was duplicated three times.
- `next` debug bool was polled in `Update`.

**Changed**
- `ObjectiveHighlight` owns its own state (`IsCurrent`, completed) and implements `IScannable`.
- `LayerHighlighter` is shared.
- `ObjectiveManager` advances on `WorldEvents.ObjectiveCompleted`.

**Best approach**
- An `ObjectiveData` ScriptableObject list per mission, so objectives become data instead of scene ordering.

### 9. Enemy AI (FSM, vision, hearing, movement, door navigation, routes)

**Right — this was the best-designed system in the project**
- State pattern with `IEnemyState`.
- `IHearable`.
- Speed-modifier stacking in `EnemyMovement`.
- Door navigation hidden behind `SetDestination` / `HasArrived`.
- Route steps as serializable data.

**Wrong**
- Public tuning fields that anything could change.
- Every state posted its own UI event (bugs #5 and #6) and repeated empty methods and the same rotation code.
- `UIManager` and `EnemyVision` type-checked `ChaseState`.
- `EnemyStepTracking` duplicated `StartStep`/`ResumeInterruptedStep`.
- `EnemyMovement` depended on the controller just to reach the agent.

**Changed**
- `EnemyStateBase` provides defaults and helpers.
- Each state exposes `Indicator`, and `ChangeState` raises it in one place.
- Tuning values are private fields with read-only properties.
- The enemy implements `IStunnable` and `IPlayerCatcher`.
- Vision has a non-alloc overlap, a coroutine handle and a correct ray length.

**Best approach**
- Move per-enemy tuning into an `EnemySettings` ScriptableObject, so every guard shares one tunable asset.
- Cache state instances (or use a small transition table) instead of `new` on every transition.
- If behaviour keeps growing, a perception "blackboard" plus a behaviour tree scales better than more FSM states.

### 10. Sound detection (`SoundEmitter`, `SoundSignal`, `IHearable`)

**Right**
- `IHearable` is a textbook small interface.
- `SoundSignal` is immutable.

**Wrong**
- The debug bool was polled in `Update`.
- No layer mask.
- An enemy with several colliders heard the same sound several times.
- A new signal object was created per listener.

**Changed**
- `[ContextMenu]` debug, `hearableMask`, non-alloc overlap, a de-dup set, and one signal per sound.

**Best approach**
- Add occlusion (walls dampen the radius) and an "intensity", so hearing is graded instead of yes/no.

### 11. Skills (`SkillManager`, `Skill`, `Tackle`, `ScannerSkill`, scan)

**Right**
- `SkillData` ScriptableObject plus a runtime `Skill` object cleanly separates data from behaviour.
- `SkillContext`.

**Wrong**
- `Skill` copied every data field into public mutable fields.
- The forced empty methods and `SkillManager`'s switch statement.
- `Tackle` did per-frame LINQ/`GetComponent`, created `StunnedState`, and could freeze the player (bug #3).
- Demon Eye lived in `PlayerController`.
- `ScanTrigger` hard-coded two concrete types.

**Changed**
- `Skill` reads its `Data` and has `OnInputPressed`/`OnInputReleased`/`Tick`.
- `Tackle` locks its target at dash start and stuns through `IStunnable`.
- `SkillManager` owns the Demon Eye toggle.
- `ScannerSkill` exposes `IsActive`/`CanActivate` with a reusable `Cooldown`.
- `ScanTrigger` works on any `IScannable`.

**Best approach**
- Make Demon Eye a `Skill` too, so equip/radial/cooldown/cost logic is shared.
- Put cooldown and cost in the `Skill` base class.

### 12. UI (`UIManager` → presenters, tracking, password, fade)

**Right**
- Event-driven panels.
- Off-screen edge clamping for tracking markers.

**Wrong**
- `UIManager` was a god class.
- `System.Security.Cryptography` was imported but unused.
- The UI type-checked enemy states.
- `FadeOutText` gave its coroutine to another object to run.
- Per-frame `SetActive` calls.

**Changed**
- `UIManager` keeps the Inspector references and builds one **presenter** per panel (Model-View-Presenter): `ReadPanelPresenter`, `TipPresenter`, `PasswordPanelPresenter`, `LightsOutPresenter`, `EnemyIndicatorPresenter`, `GameOverPresenter`.

**Best approach**
- Later, make each presenter its own component on its own panel, so the HUD can be rearranged without touching code.

### 13. Missions (`Mission`, `Mission1`, `MissionManager`, mission UI)

**Right**
- A mission abstraction with a status enum.
- A persistent manager.

**Wrong**
- The model held its UI button (`Mission.uiButton`).
- Completion was polled every frame with `FindGameObjectWithTag` (bug #4).
- `OnSubmit` threw.
- `"Mission1"` was a string literal in `HideoutMainDoor`.
- `instance` was a public mutable static field, and the duplicate kept running `Awake`.

**Changed**
- `Mission` raises `StatusChanged` (the button subscribes) and has guarded transitions.
- `Mission1` completes on `WorldEvents.ItemCollected`.
- Each mission knows its `SceneName`.
- `MissionManager.Instance` is read-only, and `Accept(mission)` sets the active mission.

**Best approach**
- `MissionData` ScriptableObjects (name, description, scene, list of objectives/conditions) instead of one C# class per mission.

### 14. Dialogue (`DialogueManager`, `DialogueView`, `DialogueInput`)

**Right**
- `DialogueData` ScriptableObjects with lines and choices.
- Thoughtful guards: wait for key release, ignore the opening frame.

**Wrong**
- 500 lines mixing UI, flow, legacy input polling and cursor control.
- NPCs called the singleton directly.
- The file encoding (bug #18).

**Changed**
- Flow stays in `DialogueManager`; drawing is in `DialogueView`; keys are in `DialogueInput` (Input System).
- The cursor goes through `InputModeService`.
- NPCs raise `UIEvents.RequestDialogue`.

**Best approach**
- For more branching, use a dialogue tool (Yarn Spinner / Ink) or a node-graph ScriptableObject, and keep `DialogueView` as the renderer.

### 15. VFX / teammate folders (`! VFX`, `! TechArtUpdate`)

- `FlashbangGrenade` now targets `IFlashable` (implemented by `PlayerFlashbangReceiver`), so enemies can react to flashbangs later.
- **These scripts are dead code and are safe to delete after you check with your team:**
  - `PlayerScannerSkill` – an old Q-key prototype.
  - `ScannableEnemy` – replaced by `ScannableObject`.
  - The `*_MadeByJay.cs` files – fully commented-out copies.
- `Player_Old.prefab` and `Mission1_Old.unity` also look obsolete.

---

## Files added

| Folder | New files |
|---|---|
| `Events/` | `UIEvents`, `PlayerEvents`, `EnemyEvents`, `WorldEvents` |
| `Player/` | `PlayerMotor`, `HealthScreenEffect`, `Input/InputModeService` |
| `Player/Inventory/` | `InventoryModel`, `IItemHolder` |
| `Player/Struggle/` | `IPlayerCatcher` |
| `Player/Skill/` | `IScannable`; `ISkillUser` (inside `SkillContext.cs`) |
| `Interactable/` | `InteractableBase`, `InteractorCapabilities` (`IInteractorBody`, `IHidingAgent`, `IPromptTarget`), `Lock/IPasswordLock` |
| `Enemy/` | `IStunnable`, `EnemyStates/EnemyStateBase` |
| `UI/Presenters/` | `IUIPresenter` + 6 presenters |
| `Dialogue/` | `DialogueView`, `DialogueInput` |
| `Utility/` | `LayerHighlighter`, `Cooldown` |
| `! VFX/` | `IFlashable` |

**Removed:** `GameEvent.cs` (replaced by the event channels) and the empty `EnemyInteraction` / `EnemyInteractionDetector` stubs.

## Things to do in the Unity Editor

1. **Assign the two item fields:**
   - `requiredKey` on `DoorParent_NormalLock_NeedKey` → `Mission1_KeyForBasement`.
   - `requiredItem` on the Mission1 leave point → `Mission1_MorseCode`.

   They work without this, but log a warning until it's done.
2. **Optional:** re-save the prefabs (or run `AssetDatabase.ForceReserializeAssets()` on them) to drop the stale YAML keys listed at the top.
3. **When convenient:** add `PlayerInventory` / `PlayerHealthEffects` components and move the matching fields onto them.

## Play-mode test checklist

- [ ] Talk to an NPC with dialogue (dialogue opens; the mouse is free; Space doesn't vault; closing gives control back without auto-walking).
- [ ] Mission NPC → accept → main door loads Mission1; without accepting it shows "Accept mission first".
- [ ] Mission1: pick up the key **and** read the code note (bug #1 fixed; scroll the wheel with two items in range).
- [ ] Basement door: from the far side it says "need key" without the key and opens with it; from the near side it says "Locked from the other side".
- [ ] Password door: wrong code shows a tip; the right code opens it **and you can still move** (bug #2).
- [ ] Circuit board → lights out, the electric door opens from both sides, enemies move to their next route step.
- [ ] Crouch while running is ignored and the next press crouches normally; Shift + crouch + release Shift + stand → walking, not running.
- [ ] Demon Eye: toggles; not while running; the FOV and grey screen fade; objectives and enemies highlight inside the wave.
- [ ] Enemy: "?" fills up while it stares at you; chase icon; give-up clears the icon.
- [ ] Get caught → mash struggle → the enemy is stunned; die in a struggle → Game Over → Restart.
- [ ] Tackle a nearby enemy (marker shows, switches cleanly between two enemies, the player never freezes).
- [ ] Locker: hide/exit with F; enemies don't see you; you can't crouch inside.
- [ ] Morse code → "Mission1 completed" shows **once**, lights come back, the leave point returns you to the Hideout.
