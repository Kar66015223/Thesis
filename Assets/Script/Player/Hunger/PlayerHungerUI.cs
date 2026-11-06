using UnityEngine;
using UnityEngine.UI;

public class PlayerHungerUI : MonoBehaviour
{
    [SerializeField] private Image hungerBar;
    private PlayerHunger hunger;

    void Awake()
    {
        hunger = GetComponentInParent<PlayerHunger>();
    }

    void Update()
    {
        hungerBar.fillAmount = hunger.Normalized;
    }
}