using UnityEngine;

public class ObjectiveChangeTrigger : MonoBehaviour
{
    [SerializeField] private ObjectiveData data;
    private bool hasRequested = false;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && data != null && !hasRequested)
        {
            UIEvents.RequestChangeObjective(data);
            hasRequested = true;
        }
    }
}
