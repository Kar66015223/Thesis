using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    [SerializeField] private float radius = 10f;
    public EnemyReaction curReaction = EnemyReaction.LookAt;

    public bool makeSound = false;

    void Update()
    {
        if(makeSound)
        {
            MakeSound(radius);
        }
    }

    public void MakeSound(float radius)
    {
        makeSound = false;
        
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        foreach (var col in colliders)
        {
            if (col.TryGetComponent(out IHearable hearable))
            {
                SoundSignal signal = new(transform.position, radius, curReaction);
                hearable.OnHearSound(signal);
            }
        }

        Debug.Log($"Sound Made by {gameObject.name}!!!");
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}