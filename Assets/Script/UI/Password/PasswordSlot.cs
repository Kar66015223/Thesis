using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PasswordSlot : MonoBehaviour
{
    public int CurrentNumber { get; private set; } = 0;

    [SerializeField] private Button increaseButton;
    [SerializeField] private Button decreaseButton;
    [SerializeField] private TMP_Text numberText;

    void Awake()
    {
        increaseButton.onClick.AddListener(IncreaseNumber);
        decreaseButton.onClick.AddListener(DecreaseNumber);

        numberText.text = CurrentNumber.ToString();
    }

    public void IncreaseNumber()
    {
        CurrentNumber += 1;

        if(CurrentNumber > 9)
        {
            CurrentNumber = 0;
        }

        numberText.text = CurrentNumber.ToString();
    }
    
    public void DecreaseNumber()
    {
        CurrentNumber -= 1;

        if (CurrentNumber < 0)
        {
            CurrentNumber = 9;
        }
        
        numberText.text = CurrentNumber.ToString();
    }
}