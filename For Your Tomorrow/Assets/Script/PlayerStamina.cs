using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    [SerializeField] private float maxStamina = 100.0f;
    [SerializeField] private float _curStamina;
    public float CurStamina
    {
        get => _curStamina;
        set => _curStamina = Mathf.Clamp(value, 0, maxStamina);
    }

    [SerializeField] private float runDrainRate = 20f;

    [SerializeField] private float regenRate = 15f;
    [Range(0, 1)][SerializeField] private float recoveryThreshold = 0.25f;

    public bool isMoving;
    public bool isRunning;
    public bool isExhausted;

    void Start()
    {
        CurStamina = maxStamina;
    }

    void Update()
    {
        Debug.Log(CurStamina);
        CalculateStamina();
    }

    private void CalculateStamina()
    {
        if (isMoving && isRunning && !isExhausted && CurStamina > 0)
        {
            CurStamina -= runDrainRate * Time.deltaTime;

            if (CurStamina == 0)
            {
                isRunning = false;
                isExhausted = true;
            }
        }
        else
        {
            CurStamina += regenRate * Time.deltaTime;

            if (isExhausted && CurStamina >= (maxStamina * recoveryThreshold))
                isExhausted = false;
        }
    }

    public void Drain(float amount)
    {
        CurStamina -= amount;
    }
}