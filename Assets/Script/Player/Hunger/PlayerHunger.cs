using UnityEngine;

public class PlayerHunger : MonoBehaviour
{
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float _curHunger;
    public float CurHunger { get => _curHunger; private set => _curHunger = Mathf.Clamp(value, 0, maxHunger); }

    [SerializeField] private float drainRate = 0.1f;
    public float Normalized => maxHunger > 0f ? CurHunger / maxHunger : 0f;

    void Start() => CurHunger = maxHunger;

    void Update()
    {
        CurHunger -= drainRate * Time.deltaTime;
    }
}