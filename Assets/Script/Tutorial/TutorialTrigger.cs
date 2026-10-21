using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [SerializeField] private TutorialData data;
    private Collider col;

    void Awake()
    {
        col = GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            UIEvents.ToggleTutorialPanel(true, data);
            if (col.enabled)
                col.enabled = false;
        }
    }
}