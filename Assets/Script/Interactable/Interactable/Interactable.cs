using TMPro;
using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable
{
    public GameObject Owner { get; set; }
    [field: SerializeField] public Transform InteractPromptPos { get; private set; }
    [SerializeField] private string interactPromptText = "[Space]";

    [SerializeField] private ObjectiveHighlight highlight;
    public bool hasInteracted = false;

    protected virtual void Awake()
    {
        Owner = gameObject;
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
        if (interactor.TryGetComponent(out PlayerController controller))
        {
            if (controller.CurrentState == PlayerState.Running ||
                controller.CurrentState == PlayerState.UsingSkill ||
                controller.CurrentState == PlayerState.Hiding)
                return false;
        }

        return true;
    }

    public virtual void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor))
            return;

        if (highlight != null && !hasInteracted && highlight.canHighlight)
        {
            GameEvent.OnChangeObjective?.Invoke();
            hasInteracted = true;
        }
    }

    public void TogglePrompt(TMP_Text prompt, bool isShow, GameObject player)
    {
        if (!CanInteract(player) && isShow)
            return;

        if (InteractPromptPos == null)
            return;

        Camera cam = Camera.main;

        // prompt.enabled = isShow;

        if (isShow)
        {
            prompt.text = interactPromptText;
            prompt.transform.position = cam.WorldToScreenPoint(InteractPromptPos.position);
        }
        else
            prompt.text = "";
    }

    private void HandleObjectiveReset(ObjectiveHighlight resetHighlight)
    {
        if (resetHighlight == highlight)
            hasInteracted = false;
    }
}