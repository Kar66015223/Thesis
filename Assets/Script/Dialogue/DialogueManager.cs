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

    [Header("Player Control")]
    [SerializeField] private Behaviour playerMovement;

    // --- State Variables ---
    private DialogueData currentDialogue;
    private DialogueStage dialogueStage;
    private int currentLineIndex;
    private bool isSequenceDialogue;

    // --- Input & Timing State ---
    private bool waitForInputRelease;
    private bool waitingForCloseClick;
    private int choiceSelectedFrame = -1;
    private int dialogueOpenedFrame = -1;

    // --- Player Original State ---
    private bool previousMovementEnabled;
    private bool previousCursorVisible;
    private CursorLockMode previousCursorLockMode;
    private bool hasSavedMovementState;

    public bool IsDialogueOpen => dialoguePanel != null && dialoguePanel.activeSelf;

    private enum DialogueStage { PlayerLine, NpcLine, ChoiceResponse, SequenceLine }

    // --- Input Properties ---
    private bool IsInputHeld => Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter);
    private bool IsAdvanceInputDown => Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
    private bool IsClickDown => Input.GetMouseButtonDown(0);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        dialoguePanel?.SetActive(false);
    }

    private void Update()
    {
        if (!IsDialogueOpen) return;

        if (waitForInputRelease)
        {
            if (!IsInputHeld) waitForInputRelease = false;
            return;
        }

        if (waitingForCloseClick)
        {
            if (Time.frameCount > choiceSelectedFrame && IsClickDown)
            {
                EndDialogue();
            }
            return;
        }

        if (Time.frameCount <= dialogueOpenedFrame) return;

        if (IsAdvanceInputDown)
        {
            if (isSequenceDialogue) AdvanceSequenceDialogue();
            else AdvanceDialogue();
        }
    }

    private void LateUpdate()
    {
        if (IsDialogueOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // =========================================
    // Core Functions
    // =========================================

    public void StartDialogue(DialogueData data)
    {
        if (data == null || dialoguePanel == null || dialoguePanel.activeSelf) return;

        SaveAndDisablePlayerState();
        dialoguePanel.SetActive(true);

        // โหลดบทสนทนาแรก พร้อมเปิดระบบป้องกันการกดปุ่มค้าง
        LoadDialogue(data, true);
    }

    // [แก้ไขแล้ว] แยกฟังก์ชัน LoadDialogue คืนมาเพื่อไม่ให้ติดเงื่อนไขป้องกันการเปิดหน้าต่างซ้อน
    private void LoadDialogue(DialogueData data, bool isFirstTime)
    {
        if (data == null) return;

        currentDialogue = data;
        ClearChoices();
        ResetDialogueState(Time.frameCount, isFirstTime);

        isSequenceDialogue = currentDialogue.conversationLines != null && currentDialogue.conversationLines.Length > 0;

        if (isSequenceDialogue)
            ShowSequenceLine();
        else
            ShowPlayerDialogue();
    }

    public void EndDialogue(bool isOpeningOtherUI = false)
    {
        dialoguePanel?.SetActive(false);
        ClearChoices();
        ResetDialogueState(-1, false);
        RestorePlayerState(isOpeningOtherUI);

        currentDialogue = null;
    }

    // =========================================
    // Sequence Dialogue (แบบต่อเนื่อง)
    // =========================================

    private void ShowSequenceLine()
    {
        if (currentDialogue?.conversationLines == null)
        {
            EndDialogue();
            return;
        }

        while (currentLineIndex < currentDialogue.conversationLines.Length && currentDialogue.conversationLines[currentLineIndex] == null)
        {
            currentLineIndex++;
        }

        if (currentLineIndex >= currentDialogue.conversationLines.Length)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = currentDialogue.conversationLines[currentLineIndex];
        dialogueStage = DialogueStage.SequenceLine;

        UpdateSpeakerName(line);

        if (dialogueText != null)
            dialogueText.text = line.dialogueText;
    }

    private void AdvanceSequenceDialogue()
    {
        currentLineIndex++;
        if (currentLineIndex < currentDialogue.conversationLines.Length)
            ShowSequenceLine();
        else
            EndDialogue();
    }

    // =========================================
    // Classic Dialogue (แบบถาม-ตอบ มีตัวเลือก)
    // =========================================

    private void ShowPlayerDialogue()
    {
        waitingForCloseClick = false;
        dialogueStage = DialogueStage.PlayerLine;
        ClearChoices();

        if (npcNameText != null)
            npcNameText.text = string.IsNullOrWhiteSpace(currentDialogue.playerName) ? "Player" : currentDialogue.playerName;

        if (dialogueText != null)
            dialogueText.text = currentDialogue.playerDialogueText;

        if (string.IsNullOrWhiteSpace(currentDialogue.playerDialogueText))
            ShowNpcDialogue();
    }

    private void ShowNpcDialogue()
    {
        dialogueStage = DialogueStage.NpcLine;

        if (npcNameText != null) npcNameText.text = currentDialogue.npcName;
        if (dialogueText != null) dialogueText.text = currentDialogue.dialogueText;

        ShowChoices();
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

            case DialogueStage.ChoiceResponse:
                break;
        }
    }

    // =========================================
    // Choices System
    // =========================================

    private void ShowChoices()
    {
        ClearChoices();

        if (currentDialogue?.choices == null || choicesParent == null || choiceButtonPrefab == null) return;

        foreach (var choice in currentDialogue.choices)
        {
            if (choice == null) continue;

            Button button = Instantiate(choiceButtonPrefab, choicesParent);
            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();

            if (buttonText != null) buttonText.text = choice.choiceText;

            button.onClick.AddListener(() => SelectChoice(choice));
        }
    }

    private void SelectChoice(DialogueChoice choice)
    {
        if (choice == null) return;
        ClearChoices();

        // 1. ถ้าเชื่อมกับระบบ Quest ให้ส่ง Event
        if (choice.openQuestUI)
        {
            GameEvent.OnToggleUIQuestNPC?.Invoke(true);
        }

        // 2. ถ้าติ๊กจบบทสนทนา
        if (choice.endDialogue)
        {
            EndDialogue(isOpeningOtherUI: choice.openQuestUI);
            return;
        }

        // 3. ถ้ามีบทสนทนาหน้าต่อไป (แก้เรียก LoadDialogue แทน)
        if (choice.nextDialogue != null)
        {
            LoadDialogue(choice.nextDialogue, false);
            return;
        }

        // 4. ถ้าไม่มีบทต่อไป ให้แสดงคำตอบแล้วรอคลิกปิด
        waitingForCloseClick = true;
        choiceSelectedFrame = Time.frameCount;
        dialogueStage = DialogueStage.ChoiceResponse;

        if (npcNameText != null) npcNameText.text = currentDialogue.npcName;
        if (dialogueText != null) dialogueText.text = choice.responseText;
    }

    private void ClearChoices()
    {
        if (choicesParent == null) return;

        foreach (Transform child in choicesParent)
        {
            if (child.TryGetComponent(out Button button)) button.interactable = false;
            Destroy(child.gameObject);
        }
    }

    // =========================================
    // Helper Methods
    // =========================================

    private void UpdateSpeakerName(DialogueLine line)
    {
        if (npcNameText == null) return;

        string speaker = line.speakerName;

        if (currentLineIndex == 0 && string.IsNullOrWhiteSpace(speaker))
            speaker = currentDialogue.playerName;

        if (string.IsNullOrWhiteSpace(speaker))
            speaker = currentDialogue.npcName;

        npcNameText.text = speaker;
    }

    private void SaveAndDisablePlayerState()
    {
        previousCursorVisible = Cursor.visible;
        previousCursorLockMode = Cursor.lockState;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerMovement != null)
        {
            previousMovementEnabled = playerMovement.enabled;
            hasSavedMovementState = true;
            playerMovement.enabled = false;
        }
    }

    private void RestorePlayerState(bool isOpeningOtherUI)
    {
        if (playerMovement != null && hasSavedMovementState)
            playerMovement.enabled = previousMovementEnabled;

        hasSavedMovementState = false;

        if (!isOpeningOtherUI)
        {
            Cursor.lockState = previousCursorLockMode;
            Cursor.visible = previousCursorVisible;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void ResetDialogueState(int frameCount, bool waitInput)
    {
        currentLineIndex = 0;
        isSequenceDialogue = false;
        waitingForCloseClick = false;
        choiceSelectedFrame = -1;
        dialogueOpenedFrame = frameCount;
        waitForInputRelease = waitInput;
    }
}