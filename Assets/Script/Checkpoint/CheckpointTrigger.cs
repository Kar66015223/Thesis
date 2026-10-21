using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    private Collider col;

    void Awake()
    {
        col = GetComponent<Collider>();
    }

    void Start()
    {
        if (spawnPoint.parent == gameObject.transform)
            spawnPoint.SetParent(null);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerEvents.RaiseCheckpoint(spawnPoint);
            if(col.enabled)
                col.enabled = false;
        }
    }
}