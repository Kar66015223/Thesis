using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [Tooltip("Layers that stop the player from interacting with something behind them (the target and the player never do).")]
    [SerializeField] private LayerMask blockingMask = Physics.DefaultRaycastLayers;
    [SerializeField] private PlayerInteractionUI ui = new();

    private readonly PlayerInteractionDetector detector = new();
    private readonly PlayerInteractionHandler handler = new();

    private PlayerController ctrl;
    private PlayerInputHandler inputHandler;

    public Transform rayStart;

    void Awake()
    {
        ctrl = GetComponentInParent<PlayerController>();
        inputHandler = GetComponentInParent<PlayerInputHandler>();
        player = ctrl.gameObject;

        detector.Initialize(
            GetComponent<Collider>(), ctrl.GetComponent<CharacterController>(), blockingMask, rayStart);
        ui.Initialize();
    }

    void OnEnable()
    {
        inputHandler.OnInteractFInput += HandleInteractF;
        inputHandler.OnInteractSpacebarInput += HandleInteractSpacebar;
        inputHandler.OnScrollInput += handler.ScrollItemSelection;
    }

    void OnDisable()
    {
        inputHandler.OnInteractFInput -= HandleInteractF;
        inputHandler.OnInteractSpacebarInput -= HandleInteractSpacebar;
        inputHandler.OnScrollInput -= handler.ScrollItemSelection;
    }

    void Update()
    {
        detector.RemoveInvalids();
        handler.Refresh(detector.Detected, player, ctrl.CanStartInteraction);
        ui.Render(handler);
    }

    void OnTriggerStay(Collider other) => detector.Add(other);
    void OnTriggerExit(Collider other) => detector.Remove(other);

    private void HandleInteractF() => TryInteract(InteractButton.F);
    private void HandleInteractSpacebar() => TryInteract(InteractButton.Space);

    private void TryInteract(InteractButton button)
    {
        if (ctrl.IsHiding)
        {
            ctrl.CurrentHidingPlace.ToggleHide(player);
            return;
        }

        if (handler.TryInteract(button, player))
            ctrl.PlayInteractAnimation();
    }
}