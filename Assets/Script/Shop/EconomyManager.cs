using System;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    public int CurrentMoney { get; private set; } = 0;
    public event Action<int> MoneyChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        // บังคับยิง Event ส่งค่าเงินตั้งต้นไปบอก UI ทันทีที่เริ่มเกม
        MoneyChanged?.Invoke(CurrentMoney);
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0) return;
        CurrentMoney += amount;
        MoneyChanged?.Invoke(CurrentMoney);
    }
}