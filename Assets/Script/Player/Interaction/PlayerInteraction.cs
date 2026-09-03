using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public PlayerInteractionDetector detector = new();
    public PlayerInteractionHandler handler = new();
    public PlayerInteractionUI ui = new();

    void Awake()
    {
        detector.Initialize(ui);
        ui.Initialize(detector, handler);
    }

    void Update()
    {
        detector.RemoveInvalids();
        ui.selection.HandleScrollSelect();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IInteractable>(out _))
        {
            detector.AddDetected(other);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IInteractable>(out _))
        {
            detector.RemoveDetected(other);
        }
    }
}