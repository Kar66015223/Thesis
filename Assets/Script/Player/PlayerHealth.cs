using Unity.VisualScripting;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float _curHP;
    public float CurHP
    {
        get => _curHP;
        set => _curHP = Mathf.Clamp(value, 0, maxHP);
    }
    [SerializeField] private float maxHP = 100f;

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

    void Awake()
    {
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
    }

    public void Heal(float amount)
    {
        takeDamage = false;
        heal = false;
        CurHP += amount;
    }

    private void UpdateScreenEffect()
    {
        if (CurHP <= 50f && CurHP > 20f)
            curVignettePower = halfHPVignettePower;
        else if (CurHP <= 20f)
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