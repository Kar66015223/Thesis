using UnityEngine;

public class TipsTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {   
        if (other.CompareTag("Player"))
        {
            GameEvent.OnShowTips?.Invoke("[Right Click] to see enemies");
            Destroy(gameObject);
        }
    }
}
