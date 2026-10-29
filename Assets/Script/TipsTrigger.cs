using UnityEngine;

public class TipsTrigger : MonoBehaviour
{
    [SerializeField, TextArea] private string message = "[Right Click] to see enemies";
    private Collider col;

    void Awake() => col = GetComponent<Collider>();

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        UIEvents.ShowTip(message);
        if (col.enabled)
            col.enabled = false;
    }
}
