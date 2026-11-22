using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerHungerUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Image hungerBar;
    [SerializeField] private TMP_Text hungerText;
    private PlayerHunger hunger;

    void Awake()
    {
        hunger = GetComponentInParent<PlayerHunger>();
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name == SceneConstants.HIDEOUT_NAME)
            root.SetActive(false);
        else
            root.SetActive(true);
    }

    void Update()
    {
        hungerBar.fillAmount = hunger.Normalized;
        hungerText.text = $"{hunger.CurHunger:F0}/{hunger.MaxHunger}";
    }
}