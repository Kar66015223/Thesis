using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private GameObject player;

    public PlayerInteractionDetector detector = new();
    public PlayerInteractionHandler handler = new();
    public PlayerInteractionUI ui = new();

    void Awake()
    {
        player = GetComponentInParent<PlayerController>().gameObject;
        Collider col = GetComponent<Collider>();

        detector.Initialize(player, ui, col);
        handler.Initialize(player);
        ui.Initialize(player, detector, handler);
    }

    void Update()
    {
        detector.RemoveInvalids();
        ui.selection.HandleScrollSelect();
    }

    void OnTriggerStay(Collider other)
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