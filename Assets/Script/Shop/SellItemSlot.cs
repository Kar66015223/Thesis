using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

public class SellItemSlot : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private Button removeButton;

    private ItemData item;
    public ItemData Item => item;
    private int amount = 0;

    public void Bind(ItemData item, int amount, Action onRemove)
    {
        if(this.item != item)
            this.item = item;
        
        if (icon != null)
            icon.sprite = item.icon;
        if (amountText != null)
            amountText.SetText($"{amount}");
        if(removeButton != null)
        {
            removeButton.onClick.RemoveAllListeners();
            removeButton.onClick.AddListener(() => onRemove?.Invoke());
        }
    }
}