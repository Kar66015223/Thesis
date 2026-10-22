using System;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    public int CurrentMoney { get; private set; } = 0; // เริ่มต้นที่ 0 บาท
    public event Action<int> MoneyChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0) return;
        CurrentMoney += amount;
        MoneyChanged?.Invoke(CurrentMoney);
    }
}