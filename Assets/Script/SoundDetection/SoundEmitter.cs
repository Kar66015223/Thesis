using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    [SerializeField] private float radius = 10f;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.M))
        {
            MakeSound(radius);
        }
    }

    public void MakeSound(float radius)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        foreach (var col in colliders)
        {
            if (col.TryGetComponent(out IHearable hearable))
            {
                SoundSignal signal = new(transform.position, radius);
                hearable.OnHearSound(signal);
            }
        }

        Debug.Log($"Sound Made by {gameObject.name}!!!");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}