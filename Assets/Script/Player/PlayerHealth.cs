using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float _curHP;
    public float CurHP
    {
        get => _curHP;
        set
        {
            _curHP = Mathf.Clamp(value, 0, maxHP);
            if (_curHP == 0)
                Die();
        } 
    }
    [SerializeField] private float maxHP = 100f;

    public bool IsDead { get; private set; } = false;

    [Header("Blood Effect")]
    [SerializeField] private Material bloodFX;
    private const string VignettePower = "_VignettePower";
    private const string BreathFrequency = "_BreathFrequency";

    private float curVignettePower;
    [SerializeField] private float normalVignettePower = 20f;
    [SerializeField] private float halfHPVignettePower = 10f;
    [SerializeField] private float lowHPVignettePower = 8f;

    private float curBreathFrequency;
    [SerializeField] private float normalBreathFrequency = 2f;
    [SerializeField] private float lowHPBreathFrequency = 5f;

    public bool takeDamage = false;
    public bool heal = false;

    private PlayerController ctrl;

    void Awake()
    {
        ctrl = GetComponent<PlayerController>();
        CurHP = maxHP;
    }

    void OnDisable()
    {
        ResetBloodFX();
    }

    void OnApplicationQuit()
    {
        ResetBloodFX();
    }

    void Update()
    {
        if (takeDamage)
            TakeDamage(10);
        if (heal)
            Heal(10);
        
        UpdateScreenEffect();
    }

    public void TakeDamage(float damage)
    {
        takeDamage = false;
        heal = false;
        CurHP -= damage;
        Debug.Log($"{gameObject.name} take {damage} damage");
    }

    public void Heal(float amount)
    {
        takeDamage = false;
        heal = false;
        CurHP += amount;
    }

    public void Die()
    {
        IsDead = true;
        GameEvent.OnToggleGameOverUI?.Invoke(true);
        ctrl.SetCanMove(false);
    }

    private void UpdateScreenEffect()
    {
        if (CurHP < maxHP && CurHP > 50f)
            curVignettePower = halfHPVignettePower;
        else if (CurHP <= 50f)
        {
            curVignettePower = lowHPVignettePower;
            curBreathFrequency = lowHPBreathFrequency;
        }
        else
        {
            curVignettePower = normalVignettePower;
            curBreathFrequency = normalBreathFrequency;
        }

        bloodFX.SetFloat(VignettePower, curVignettePower);
        bloodFX.SetFloat(BreathFrequency, curBreathFrequency);
    }
    
    private void ResetBloodFX()
    {
        bloodFX.SetFloat(VignettePower, normalVignettePower);
        bloodFX.SetFloat(BreathFrequency, normalBreathFrequency);
    }
}