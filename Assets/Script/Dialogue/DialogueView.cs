using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueView
{
    private readonly GameObject panel;
    private readonly TMP_Text speakerText;
    private readonly TMP_Text bodyText;
    private readonly Transform choicesParent;
    private readonly Button choiceButtonPrefab;

    public DialogueView(GameObject panel, TMP_Text speakerText, TMP_Text bodyText, Transform choicesParent,
        Button choiceButtonPrefab)
    {
        this.panel = panel;
        this.speakerText = speakerText;
        this.bodyText = bodyText;
        this.choicesParent = choicesParent;
        this.choiceButtonPrefab = choiceButtonPrefab;
    }

    public bool IsValid => panel != null;
    public bool IsVisible => panel != null && panel.activeSelf;

    public void SetVisible(bool IsVisible)
    {
        panel.SetActive(IsVisible);
    }

    public void ShowLine(string speaker, string text)
    {
        speakerText.text = speaker;
        bodyText.text = text;
    }

    public void ShowChoices(IEnumerable<DialogueChoice> choices, Action<DialogueChoice> onSelected)
    {
        ClearChoices();

        if (choices == null || choicesParent == null || choiceButtonPrefab == null)
            return;

        foreach (DialogueChoice choice in choices)
        {
            if (choice == null)
                continue;

            Button button = UnityEngine.Object.Instantiate(choiceButtonPrefab, choicesParent);

            TMP_Text label = button.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = choice.choiceText;

            DialogueChoice selected = choice;
            button.onClick.AddListener(() => onSelected(selected));
        }
    }
    
    public void ClearChoices()
    {
        if (choicesParent == null)
            return;

        foreach(Transform child in choicesParent)
        {
            if (child.TryGetComponent(out Button button))
                button.interactable = false;

            UnityEngine.Object.Destroy(child.gameObject);
        }
    }
}