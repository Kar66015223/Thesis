using UnityEngine;

public class ScanInteraction : MonoBehaviour
{
    [SerializeField] private ScanInteractionDetector detector;

    void Awake()
    {
        detector.Initialize();
    }

    void Update()
    {
        detector.CheckInteractable();
    }
}