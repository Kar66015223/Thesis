using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHungerUI : MonoBehaviour
{
    [SerializeField] private Image hungerBar;
    [SerializeField] private TMP_Text hungerText;
    private PlayerHunger hunger;

    void Awake()
    {
        hunger = GetComponentInParent<PlayerHunger>();
    }

    void Update()
    {
        hungerBar.fillAmount = hunger.Normalized;
        hungerText.text = $"{hunger.CurHunger:0.#}/{hunger.MaxHunger}";
    }
}