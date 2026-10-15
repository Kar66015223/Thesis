using UnityEngine;

/// <summary>
/// Coordinates the interaction feature: detector (what overlaps) -> handler (what is selected)
/// -> UI (how it looks), and turns F / Space / scroll input into interactions.
/// Moved here from PlayerController so the controller only deals with movement.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private PlayerInteractionUI ui = new();

    private readonly PlayerInteractionDetector detector = new();
    private readonly PlayerInteractionHandler handler = new();

    private PlayerController controller;
    private PlayerInputHandler inputHandler;

    void Awake()
    {
        controller = GetComponentInParent<PlayerController>();
        inputHandler = GetComponentInParent<PlayerInputHandler>();
        player = controller.gameObject;

        detector.Initialize(GetComponent<Collider>());
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
        handler.Refresh(detector.Detected, player, controller.CanStartInteraction);
        ui.Render(handler);
    }

    void OnTriggerStay(Collider other) => detector.Add(other);
    void OnTriggerExit(Collider other) => detector.Remove(other);

    private void HandleInteractF() => TryInteract(InteractButton.F);
    private void HandleInteractSpacebar() => TryInteract(InteractButton.Spacebar);

    private void TryInteract(InteractButton button)
    {
        // While hiding, any interact button leaves the hiding spot.
        if (controller.IsHiding)
        {
            controller.CurrentHidingPlace.ToggleHide(player);
            return;
        }

        if (handler.TryInteract(button, player))
            controller.PlayInteractAnimation();
    }
}
