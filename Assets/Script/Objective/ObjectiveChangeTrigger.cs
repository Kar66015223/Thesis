using UnityEngine;

public class ObjectiveChangeTrigger : MonoBehaviour
{
    [SerializeField] private ObjectiveData data;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && data != null)
            UIEvents.RequestChangeObjective(data);
    }
}
