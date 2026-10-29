using System;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    public int CurrentMoney { get; private set; }
    public event Action<int> MoneyChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0)
            return;

        SetMoney(CurrentMoney + amount);
    }

    public void RestoreMoney(int amount) => SetMoney(Mathf.Max(0, amount));

    private void SetMoney(int amount)
    {
        CurrentMoney = amount;
        MoneyChanged?.Invoke(CurrentMoney);
    }
}
