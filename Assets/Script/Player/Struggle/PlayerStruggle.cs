using System;
using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerStruggle : MonoBehaviour
{
    [SerializeField] private float struggleThreshold = 100f;
    [SerializeField] private float struggleValue = 10f;

    private float struggleProgress;
    private float damageTimer;

    private PlayerHealth hp;
    private PlayerInputHandler inputHandler;
    private PlayerController body;

    // While held: the enemy's hand and the player's neck, pinned together every frame.
    private Transform holdPoint;
    private Transform neck;
    private bool isHeld;

    public bool IsCaught { get; private set; }
    /// <summary>Caught and in the strangle loop: struggle presses count and the catcher deals damage.</summary>
    public bool IsStruggling { get; private set; }
    public IPlayerCatcher CatchingEnemy { get; private set; }
    /// <summary>Set by skills (e.g. mid-tackle) so enemies in reach can't grab the player.</summary>
    public bool IsCatchImmune { get; private set; }
    public bool CanBeCaught => !IsCaught && !IsCatchImmune && !hp.IsDead;

    /// <summary>The GotCaught animation has played to the end (see <see cref="GotCaughtStateBehavior"/>).</summary>
    public event Action GotCaughtFinished;

    void Awake()
    {
        hp = GetComponent<PlayerHealth>();
        inputHandler = GetComponent<PlayerInputHandler>();
        body = GetComponent<PlayerController>();
    }

    void OnEnable()
    {
        inputHandler.OnStruggleInput += HandleStruggleInput;
    }

    void OnDisable()
    {
        inputHandler.OnStruggleInput -= HandleStruggleInput;
        if (IsCaught)
            InputModeService.SetStruggling(false);
    }

    void Update()
    {
        if (!IsCaught || !IsStruggling)
            return;

        damageTimer += Time.deltaTime;
        if (damageTimer >= CatchingEnemy.DamageInterval)
        {
            damageTimer = 0f;
            hp.TakeDamage(CatchingEnemy.CatchDamage);
            CatchingEnemy.OnCatchDamageDealt();
        }

        if (hp.IsDead)
            EndStruggle(playerEscaped: false);
        else if (struggleProgress >= struggleThreshold)
            EndStruggle(playerEscaped: true);
    }

    // After the Animators have posed both characters this frame: move the whole player so its neck sits in the hand.
    void LateUpdate()
    {
        if (isHeld)
            PinNeckToHold();
    }

    public void SetCatchImmune(bool isImmune) => IsCatchImmune = isImmune;

    /// <summary>
    /// The catcher's grab animation has reached the player: puts the neck in <paramref name="hand"/>, keeps it there
    /// until the struggle ends, and plays GotCaught (<see cref="GotCaughtFinished"/> is raised when it's done).
    /// </summary>
    public void BeginHold(Transform hand, Vector3 catcherPosition)
    {
        if (!IsCaught || isHeld || body == null)
            return;

        isHeld = true;
        holdPoint = hand;
        neck = body.GetBone(HumanBodyBones.Neck);

        bool isPlaying = body.BeginCaught(catcherPosition);
        PinNeckToHold();

        // No GotCaught animation to wait for (no Caught controller assigned): go straight on to the loop.
        if (!isPlaying)
            NotifyGotCaughtAnimationFinished();
    }

    /// <summary>
    /// GotCaught is over: loop the caught animation and start the struggle itself. Only from here do presses
    /// count and the catcher deal damage, so the player can't break free before the hand has even reached them.
    /// </summary>
    public void StartCaughtLoop()
    {
        if (!IsCaught || IsStruggling)
            return;

        if (isHeld)
            body.PlayCaughtLoop();

        IsStruggling = true;
        damageTimer = 0f;
        PlayerEvents.RaiseStruggleToggled(true);
        PlayerEvents.RaiseStruggleProgressChanged(0f);
    }

    public void NotifyGotCaughtAnimationFinished() => GotCaughtFinished?.Invoke();

    private void PinNeckToHold()
    {
        if (holdPoint == null || neck == null)
            return;

        transform.position += holdPoint.position - neck.position;
    }

    private void EndHold()
    {
        if (!isHeld)
            return;

        isHeld = false;
        holdPoint = null;
        neck = null;
        body.EndCaught();
    }

    /// <summary>Returns false if the player can't be caught right now (already caught, immune or dead).</summary>
    public bool TryBeginStruggle(IPlayerCatcher catcher)
    {
        if (catcher == null || !CanBeCaught)
            return false;

        IsCaught = true;
        CatchingEnemy = catcher;
        struggleProgress = 0f;
        damageTimer = 0f;

        // Lock the controls now (the Struggle map has no movement), but the struggle prompt and the struggle
        // itself wait for StartCaughtLoop, once the grab animations have played.
        InputModeService.SetStruggling(true);

        return true;
    }

    private void HandleStruggleInput()
    {
        if (!IsCaught || !IsStruggling)
            return;

        struggleProgress += struggleValue;
        PlayerEvents.RaiseStruggleProgressChanged(Mathf.Clamp01(struggleProgress / struggleThreshold));
    }
    
    private void EndStruggle(bool playerEscaped)
    {
        IPlayerCatcher catcher = CatchingEnemy;

        IsCaught = false;
        IsStruggling = false;
        CatchingEnemy = null;
        struggleProgress = 0f;

        // Let go before telling the catcher, so the player is back on its feet whatever state the enemy goes to.
        EndHold();

        InputModeService.SetStruggling(false);
        PlayerEvents.RaiseStruggleToggled(false);

        catcher.ReleasePlayer(playerEscaped);
    }
}