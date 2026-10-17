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

    [Header("Player Control")]
    [SerializeField] private Behaviour playerMovement;

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

    // ระบบบทสนทนาต่อเนื่อง (sequential conversation)
    private bool isSequenceDialogue;
    private int currentLineIndex;

    // สถานะก่อนเริ่มสนทนา (state saved before the conversation)
    private bool previousMovementEnabled;
    private bool hasSavedMovementState;

    // สถานะหลังเลือกคำตอบ (after picking a choice with no follow-up: click to close)
    private bool waitingForCloseClick;
    private int choiceSelectedFrame = -1;

    // ป้องกันการกดปุ่มเริ่มสนทนาแล้วข้ามประโยคแรกทันที (don't skip the first line with the key that opened it)
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

        // รอให้ปล่อยปุ่มที่ใช้เปิด Dialogue ก่อน (wait until the opening key is released)
        if (waitForInputRelease)
        {
            if (!DialogueInput.AnyHeld)
                waitForInputRelease = false;

            return;
        }

        // หลังเลือกคำตอบที่ไม่มีบทสนทนาถัดไป คลิกเมาส์อีกครั้งเพื่อปิด Panel
        if (waitingForCloseClick)
        {
            if (Time.frameCount > choiceSelectedFrame && DialogueInput.ClickPressed)
                EndDialogue();

            return;
        }

        // ป้องกันการรับคลิกในเฟรมที่เริ่มบทสนทนา (ignore the click from the opening frame)
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
        // ป้องกันการเปิด Dialogue ซ้อน (no nested dialogues)
        if (data == null || view == null || !view.IsValid || IsDialogueOpen)
            return;

        // หยุดการเคลื่อนที่ของ Player (freeze the player)
        hasSavedMovementState = playerMovement != null;
        if (hasSavedMovementState)
        {
            previousMovementEnabled = playerMovement.enabled;
            playerMovement.enabled = false;
        }

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

        // คืนสถานะการเดินเดิม (restore player movement)
        if (playerMovement != null && hasSavedMovementState)
            playerMovement.enabled = previousMovementEnabled;
        hasSavedMovementState = false;

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

        // ข้ามบรรทัดที่ไม่ได้กำหนดข้อมูล (skip empty entries)
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
        // บรรทัดแรกใช้ชื่อจาก Player Name, บรรทัดถัดไปใช้ Speaker Name, ถ้าไม่มีชื่อให้ใช้ชื่อ NPC
        string speaker = currentLineIndex == 0 ? currentDialogue.playerName : line.speakerName;

        if (currentLineIndex == 0 && string.IsNullOrWhiteSpace(speaker))
            speaker = line.speakerName;

        return string.IsNullOrWhiteSpace(speaker) ? currentDialogue.npcName : speaker;
    }

    private void AdvanceSequenceDialogue()
    {
        currentLineIndex++;

        // อ่านครบทุกบรรทัดแล้วปิด Panel
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
        switch(dialogueStage)
        {
            case DialogueStage.PlayerLine:
                // แสดงบทพูด NPC หลังจาก Player พูด
                ShowNpcDialogue();
                break;

            case DialogueStage.NpcLine:
                // ถ้าไม่มีตัวเลือก คลิกอีกครั้งเพื่อปิด
                if (currentDialogue.choices == null || currentDialogue.choices.Length == 0)
                    EndDialogue();
                break;

            // ChoiceResponse: closing is handled in Update().
        }
    }

    private void SelectChoice(DialogueChoice choice)
    {
        if (choice == null || currentDialogue == null)
            return;

        // ป้องกันการกดตัวเลือกซ้ำ
        view.ClearChoices();

        if (choice.endDialogue)
        {
            EndDialogue();
            return;
        }

        // ถ้ามีบทสนทนาถัดไป ให้เปลี่ยนทันที
        if (choice.nextDialogue != null)
        {
            LoadDialogue(choice.nextDialogue);
            return;
        }

        // ไม่มีบทสนทนาถัดไป: แสดงคำตอบและรอคลิกเมาส์เพื่อปิด
        waitingForCloseClick = true;
        choiceSelectedFrame = Time.frameCount;
        dialogueStage = DialogueStage.ChoiceResponse;
        view.ShowLine(currentDialogue.npcName, choice.responseText);
    }
    
    #endregion
}