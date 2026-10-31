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

    private Transform holdPoint;
    private Transform neck;
    private bool isHeld;

    public bool IsCaught { get; private set; }
    public bool IsStruggling { get; private set; }
    public IPlayerCatcher CatchingEnemy { get; private set; }
    public bool IsCatchImmune { get; private set; }
    public bool CanBeCaught => !IsCaught && !IsCatchImmune && !hp.IsDead;

    public event Action Caught;
    public event Action<bool> Struggle;

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

    void LateUpdate()
    {
        if (isHeld)
            PinNeckToHold();
    }

    public void SetCatchImmune(bool isImmune) => IsCatchImmune = isImmune;

    public void BeginHold(Transform hand, Vector3 catcherPosition)
    {
        if (!IsCaught || isHeld || body == null)
            return;

        isHeld = true;
        holdPoint = hand;
        neck = body.GetBone(HumanBodyBones.Neck);

        bool isPlaying = body.BeginCaught(catcherPosition);
        PinNeckToHold();

        if (!isPlaying)
            NotifyGotCaughtAnimationFinished();
    }

    public void StartCaughtLoop()
    {
        if (!IsCaught || IsStruggling)
            return;

        if (isHeld)
            body.PlayCaughtLoop();

        IsStruggling = true;
        Struggle?.Invoke(true);
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

    public bool TryBeginStruggle(IPlayerCatcher catcher)
    {
        if (catcher == null || !CanBeCaught)
            return false;

        IsCaught = true;
        Caught?.Invoke();
        CatchingEnemy = catcher;
        struggleProgress = 0f;
        damageTimer = 0f;

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
        Struggle?.Invoke(false);
        CatchingEnemy = null;
        struggleProgress = 0f;

        EndHold();

        InputModeService.SetStruggling(false);
        PlayerEvents.RaiseStruggleToggled(false);

        catcher.ReleasePlayer(playerEscaped);
    }
}