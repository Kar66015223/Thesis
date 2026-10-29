using TMPro;
using UnityEngine;

public class InteractItemUI : MonoBehaviour
{
    [SerializeField] private GameObject selectedBG;
    [SerializeField] private TMP_Text texts;
    [SerializeField] private Color selectedTextColor = Color.black;
    [SerializeField] private Color normalTextColor = Color.white;

    private bool? isSelected;

    public void SetLabel(string label)
    {
        TMP_Text nameText = GetComponentInChildren<TMP_Text>();
        if (nameText != null)
            nameText.text = label;
    }

    public void SetSelected(bool selected)
    {
        if (isSelected == selected)
            return;

        isSelected = selected;

        if (selectedBG != null)
            selectedBG.SetActive(selected);

        if (texts == null)
            return;

        Color color = selected ? selectedTextColor : normalTextColor;
        foreach (TMP_Text text in texts.GetComponentsInChildren<TMP_Text>())
            text.color = color;
    }
}
