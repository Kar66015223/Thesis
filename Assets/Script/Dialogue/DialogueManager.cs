
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text npcNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Transform choicesParent;
    [SerializeField] private Button choiceButtonPrefab;

    private DialogueData currentDialogue;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    public void StartDialogue(DialogueData data)
    {
        if (data == null || dialoguePanel == null)
            return;

        currentDialogue = data;

        dialoguePanel.SetActive(true);

        npcNameText.text = data.npcName;
        dialogueText.text = data.dialogueText;

        ShowChoices();
    }

    private void ShowChoices()
    {
        ClearChoices();

        if (currentDialogue.choices == null)
            return;

        foreach (DialogueChoice choice in currentDialogue.choices)
        {
            Button button = Instantiate(
                choiceButtonPrefab,
                choicesParent
            );

            TMP_Text buttonText =
                button.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
                buttonText.text = choice.choiceText;

            DialogueChoice selectedChoice = choice;

            button.onClick.AddListener(() =>
            {
                SelectChoice(selectedChoice);
            });
        }
    }

    private void SelectChoice(DialogueChoice choice)
    {
        dialogueText.text = choice.responseText;

        ClearChoices();

        Button closeButton = Instantiate(
            choiceButtonPrefab,
            choicesParent
        );

        TMP_Text buttonText =
            closeButton.GetComponentInChildren<TMP_Text>();

        if (buttonText != null)
            buttonText.text = "ปิดบทสนทนา";

        closeButton.onClick.AddListener(EndDialogue);
    }

    private void ClearChoices()
    {
        foreach (Transform child in choicesParent)
        {
            Destroy(child.gameObject);
        }
    }

    public void EndDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        currentDialogue = null;
    }
}
