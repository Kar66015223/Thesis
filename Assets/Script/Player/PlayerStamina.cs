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
    public bool isInputtingRun;
    public bool IsRunning { get; private set; }
    public bool isExhausted;

    private PlayerController controller;
    private PlayerSound sound;

    void Awake()
    {
        controller = GetComponent<PlayerController>();
        sound = GetComponent<PlayerSound>();
    }

    void Start()
    {
        CurStamina = maxStamina;
    }

    void Update()
    {
        CalculateStamina();
    }

    private void CalculateStamina()
    {
        IsRunning = isMoving && isInputtingRun && !isExhausted && CurStamina > 0;

        if (IsRunning)
        {
            CurStamina -= runDrainRate * Time.deltaTime;

            if (CurStamina == 0)
            {
                IsRunning = false;
                isExhausted = true;
                sound.PlayTiredSound();
            }
        }
        else
        {
            CurStamina += regenRate * Time.deltaTime;

            if (isExhausted && CurStamina >= (maxStamina * recoveryThreshold))
                isExhausted = false;
        }
    }
}