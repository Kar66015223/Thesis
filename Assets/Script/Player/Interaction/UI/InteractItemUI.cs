using TMPro;
using UnityEngine;

public class InteractItemUI : MonoBehaviour
{
    [SerializeField] private GameObject selectedBG;
    [SerializeField] private TMP_Text texts;

    public bool isSelected = false;

    void Update()
    {
        if (isSelected)
            ChangeVisualToSelected();
        else
            ChangeVisualToNotSelected();
    }

    private void ChangeVisualToSelected()
    {
        selectedBG.SetActive(true);
        TMP_Text[] allTexts = texts.GetComponentsInChildren<TMP_Text>();
        foreach (TMP_Text text in allTexts)
        {
            text.color = Color.black;
        }
    }
    
    private void ChangeVisualToNotSelected()
    {
        selectedBG.SetActive(false);
        TMP_Text[] allTexts = texts.GetComponentsInChildren<TMP_Text>();
        foreach (TMP_Text text in allTexts)
        {
            text.color = Color.white;
        }
    }
}
