using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text npcNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Transform choicesParent;
    [SerializeField] private Button choiceButtonPrefab;

    private const string DefaultPlayerName = "Player";

    private enum DialogueStage
    {
        PlayerLine,
        NpcLine,
        ChoiceResponse,
        SequenceLine
    }

    private DialogueView view;
    private DialogueData currentDialogue;
    private DialogueStage dialogueStage;

    private bool isSequenceDialogue;
    private int currentLineIndex;

    private bool waitingForCloseClick;
    private int choiceSelectedFrame = -1;

    private bool waitForInputRelease;
    private int dialogueOpenedFrame = -1;

    public bool IsDialogueOpen => view != null && view.IsVisible;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        view = new(dialoguePanel, npcNameText, dialogueText, choicesParent, choiceButtonPrefab);
        view.SetVisible(false);
    }

    void OnEnable() => UIEvents.DialogueRequested += StartDialogue;

    void OnDisable()
    {
        UIEvents.DialogueRequested -= StartDialogue;
        InputModeService.RequestUI(this, false);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Update()
    {
        if (!IsDialogueOpen)
            return;

        if (waitForInputRelease)
        {
            if (!DialogueInput.AnyHeld)
                waitForInputRelease = false;

            return;
        }

        if (waitingForCloseClick)
        {
            if (Time.frameCount > choiceSelectedFrame && DialogueInput.ClickPressed)
                EndDialogue();

            return;
        }

        bool clicked = Time.frameCount > dialogueOpenedFrame && DialogueInput.ClickPressed;
        if (!clicked && !DialogueInput.ContinuePressed)
            return;

        if (isSequenceDialogue)
            AdvanceSequenceDialogue();
        else
            AdvanceDialogue();
    }

    public void StartDialogue(DialogueData data)
    {
        if (data == null || view == null || !view.IsValid || IsDialogueOpen)
            return;

        InputModeService.RequestUI(this, true);

        waitForInputRelease = true;
        view.SetVisible(true);
        LoadDialogue(data);
    }

    public void EndDialogue()
    {
        view.SetVisible(false);
        view.ClearChoices();

        currentDialogue = null;
        isSequenceDialogue = false;
        currentLineIndex = 0;
        waitingForCloseClick = false;
        choiceSelectedFrame = -1;
        dialogueOpenedFrame = -1;
        waitForInputRelease = false;

        InputModeService.RequestUI(this, false);
    }

    private void LoadDialogue(DialogueData data)
    {
        currentDialogue = data;
        currentLineIndex = 0;
        waitingForCloseClick = false;
        choiceSelectedFrame = -1;
        dialogueOpenedFrame = Time.frameCount;

        view.ClearChoices();

        isSequenceDialogue = data.conversationLines != null && data.conversationLines.Length > 0;

        if (isSequenceDialogue)
            ShowSequenceLine();
        else
            ShowPlayerDialogue();
    }

    #region Sequential conversation (no choices)

    private void ShowSequenceLine()
    {
        DialogueLine[] lines = currentDialogue.conversationLines;

        while (currentLineIndex < lines.Length && lines[currentLineIndex] == null)
            currentLineIndex++;

        if (currentLineIndex >= lines.Length)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = lines[currentLineIndex];
        dialogueStage = DialogueStage.SequenceLine;
        view.ShowLine(GetSequenceSpeaker(line), line.dialogueText);
    }

    private string GetSequenceSpeaker(DialogueLine line)
    {
        string speaker = currentLineIndex == 0 ? currentDialogue.playerName : line.speakerName;

        if (currentLineIndex == 0 && string.IsNullOrWhiteSpace(speaker))
            speaker = line.speakerName;

        return string.IsNullOrWhiteSpace(speaker) ? currentDialogue.npcName : speaker;
    }

    private void AdvanceSequenceDialogue()
    {
        currentLineIndex++;

        if (currentLineIndex < currentDialogue.conversationLines.Length)
            ShowSequenceLine();
        else
            EndDialogue();
    }

    #endregion

    #region Player line -> NPC line -> choices

    private void ShowPlayerDialogue()
    {
        waitingForCloseClick = false;
        dialogueStage = DialogueStage.PlayerLine;
        view.ClearChoices();

        if (string.IsNullOrWhiteSpace(currentDialogue.playerDialogueText))
        {
            ShowNpcDialogue();
            return;
        }

        string playerName = string.IsNullOrWhiteSpace(currentDialogue.playerName)
            ? DefaultPlayerName
            : currentDialogue.playerName;

        view.ShowLine(playerName, currentDialogue.playerDialogueText);
    }

    private void ShowNpcDialogue()
    {
        dialogueStage = DialogueStage.NpcLine;
        view.ShowLine(currentDialogue.npcName, currentDialogue.dialogueText);
        view.ShowChoices(currentDialogue.choices, SelectChoice);
    }

    private void AdvanceDialogue()
    {
        switch (dialogueStage)
        {
            case DialogueStage.PlayerLine:
                ShowNpcDialogue();
                break;

            case DialogueStage.NpcLine:
                if (currentDialogue.choices == null || currentDialogue.choices.Length == 0)
                    EndDialogue();
                break;
        }
    }

    private void SelectChoice(DialogueChoice choice)
    {
        if (choice == null || currentDialogue == null)
            return;

        view.ClearChoices();

        if (choice.openShopUI)
        {
            EndDialogue();
            UIEvents.ToggleShopBoard(true);
            return;
        }

        if (choice.endDialogue)
        {
            EndDialogue();
            return;
        }

        if (choice.nextDialogue != null)
        {
            LoadDialogue(choice.nextDialogue);
            return;
        }

        waitingForCloseClick = true;
        choiceSelectedFrame = Time.frameCount;
        dialogueStage = DialogueStage.ChoiceResponse;
        view.ShowLine(currentDialogue.npcName, choice.responseText);
    }

    #endregion
}