using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [SerializeField] private TutorialData data;

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
            UIEvents.ToggleTutorialPanel(true, data);
    }
}