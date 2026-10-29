using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PasswordSlot : MonoBehaviour
{
    private const int DigitCount = 10;

    [SerializeField] private Button increaseButton;
    [SerializeField] private Button decreaseButton;
    [SerializeField] private TMP_Text numberText;

    public int CurrentNumber { get; private set; }

    void Awake()
    {
        increaseButton.onClick.AddListener(IncreaseNumber);
        decreaseButton.onClick.AddListener(DecreaseNumber);
        Refresh();
    }

    public void IncreaseNumber() => Step(+1);
    public void DecreaseNumber() => Step(-1);

    private void Step(int delta)
    {
        CurrentNumber = (CurrentNumber + delta + DigitCount) % DigitCount;
        Refresh();
    }

    private void Refresh() => numberText.text = CurrentNumber.ToString();
}
