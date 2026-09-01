using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public PlayerInteractionDetector detector = new();
    public PlayerInteractionHandler handler = new();
    public PlayerInteractionUI ui = new();

    void Awake()
    {
        ui.Initialize(this, detector);
    }
}