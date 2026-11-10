using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SellItemRow : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button sellButton;

    private ItemData item;
    public ItemData Item => item;
    private int amount = 0;

    public void Bind(ItemData item, int amount, Action onSell)
    {
        if(this.item != item)
            this.item = item;
        
        if (icon != null)
            icon.sprite = item.icon;
        if (nameText != null)
            nameText.text = $"{item.itemName} (x{amount})";
        if (priceText != null)
            priceText.text = $"Price: {item.sellPrice * amount} G";

        if (sellButton != null)
        {
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(() => onSell?.Invoke());
        }
    }
}
