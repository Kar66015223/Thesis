using UnityEngine;

public class Item : MonoBehaviour, IInteractable
{
    public GameObject Owner { get; set; }
    [field: SerializeField] public ItemData Data { get; private set; }

    private InteractButton _interactButton;
    public InteractButton InteractButton { get => _interactButton; set => _interactButton = value; }

    [SerializeField] private ObjectiveHighlight highlight;
    [HideInInspector] public bool hasInteracted = false;

    void Awake()
    {
        Owner = gameObject;
        InteractButton = InteractButton.F;
    }

    void OnEnable()
    {
        GameEvent.OnObjectiveReset += HandleObjectiveReset;
    }

    void OnDisable()
    {
        GameEvent.OnObjectiveReset -= HandleObjectiveReset;
    }

    public virtual bool CanInteract(GameObject interactor)
    {
        if (interactor != null)
        {
            if (interactor.TryGetComponent(out PlayerController controller))
            {
                if (controller.CurrentState == PlayerState.Running ||
                    controller.CurrentState == PlayerState.UsingSkill ||
                    controller.CurrentState == PlayerState.Hiding)
                    return false;
            }

            // Inventory inv = interactor.GetComponentInChildren<Inventory>();
            // if (inv == null || Data == null)
            //     return false;

        }

        return true;
    }

    public virtual void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor))
            return;

        Debug.Log($"{Owner.name} interacted");

        if (highlight != null && !hasInteracted && highlight.canHighlight)
            GameEvent.OnChangeObjective?.Invoke();
    }

    private void HandleObjectiveReset(ObjectiveHighlight resetHighlight)
    {
        if (resetHighlight == highlight)
            hasInteracted = false;
    }
}