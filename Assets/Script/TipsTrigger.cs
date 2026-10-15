using UnityEngine;

/// <summary>Shows a one-time hint when the player walks in. The message is set per trigger in the Inspector.</summary>
public class TipsTrigger : MonoBehaviour
{
    [SerializeField, TextArea] private string message = "[Right Click] to see enemies";

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        UIEvents.ShowTip(message);
        Destroy(gameObject);
    }
}
