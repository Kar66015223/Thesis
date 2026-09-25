using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    [SerializeField] private float curRadius = 0f;
    public EnemyReaction curReaction;

    public bool makeSound = false;

    void Update()
    {
        if(makeSound)
        {
            MakeSound(10f, curReaction);
        }
    }

    public void MakeSound(float radius, EnemyReaction reaction)
    {
        makeSound = false;

        if (curRadius != radius)
            curRadius = radius;

        if (curReaction != reaction)
            curReaction = reaction;
        
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        foreach (var col in colliders)
        {
            if (col.TryGetComponent(out IHearable hearable))
            {
                SoundSignal signal = new(transform.position, radius, reaction);
                hearable.OnHearSound(signal);
            }
        }

        // Debug.Log($"Sound Made by {gameObject.name}!!!");
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, curRadius);
    }
#endif
}