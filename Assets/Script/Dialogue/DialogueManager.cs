
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

    private DialogueData currentDialogue;

    private enum DialogueStage
    {
        PlayerLine,
        NpcLine,
        ChoiceResponse,
        SequenceLine
    }

    private DialogueStage dialogueStage;

    // สถานะก่อนเริ่มสนทนา
    private bool previousMovementEnabled;
    private bool previousCursorVisible;
    private CursorLockMode previousCursorLockMode;
    private bool hasSavedMovementState;

    // สถานะหลังเลือกคำตอบ
    private bool waitingForCloseClick;
    private int choiceSelectedFrame = -1;

    // ป้องกันการกดปุ่มเริ่มสนทนาแล้วข้ามประโยคแรกทันที
    private bool waitForInputRelease;

    // ป้องกันการรับอินพุตในเฟรมที่เริ่มสนทนา
    private int dialogueOpenedFrame = -1;

    // ระบบบทสนทนาต่อเนื่อง
    private bool isSequenceDialogue;
    private int currentLineIndex;

    public bool IsDialogueOpen =>
        dialoguePanel != null && dialoguePanel.activeSelf;

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

    private void Update()
    {
        if (!IsDialogueOpen)
            return;

        // รอให้ปล่อยปุ่มที่ใช้เปิด Dialogue ก่อน
        if (waitForInputRelease)
        {
            bool inputHeld =
                Input.GetMouseButton(0) ||
                Input.GetKey(KeyCode.Space) ||
                Input.GetKey(KeyCode.Return) ||
                Input.GetKey(KeyCode.KeypadEnter);

            if (!inputHeld)
                waitForInputRelease = false;

            return;
        }

        // หลังเลือกคำตอบที่ไม่มีบทสนทนาถัดไป
        // คลิกเมาส์อีกครั้งเพื่อปิด Panel
        if (waitingForCloseClick)
        {
            if (Time.frameCount > choiceSelectedFrame &&
                Input.GetMouseButtonDown(0))
            {
                EndDialogue();
            }

            return;
        }

        // ป้องกันการรับคลิกในเฟรมที่เริ่มบทสนทนา
        bool clicked =
            Time.frameCount > dialogueOpenedFrame &&
            Input.GetMouseButtonDown(0);

        bool pressedContinue =
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter);

        // บทสนทนาต่อเนื่องแบบไม่มีตัวเลือก
        if (isSequenceDialogue)
        {
            if (clicked || pressedContinue)
                AdvanceSequenceDialogue();

            return;
        }

        // ระบบบทสนทนาเดิม
        if (clicked || pressedContinue)
            AdvanceDialogue();
    }

    private void LateUpdate()
    {
        // แสดงเมาส์ตลอดเวลาที่เปิด Dialogue
        if (IsDialogueOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void StartDialogue(DialogueData data)
    {
        if (data == null || dialoguePanel == null)
            return;

        // ป้องกันการเปิด Dialogue ซ้อน
        if (dialoguePanel.activeSelf)
            return;

        // บันทึกสถานะเมาส์เดิม
        previousCursorVisible = Cursor.visible;
        previousCursorLockMode = Cursor.lockState;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // หยุดการเคลื่อนที่ของ Player
        hasSavedMovementState = false;

        if (playerMovement != null)
        {
            previousMovementEnabled = playerMovement.enabled;
            hasSavedMovementState = true;
            playerMovement.enabled = false;
        }

        // ป้องกันอินพุตที่ใช้เปิดบทสนทนาถูกอ่านซ้ำ
        waitForInputRelease = true;
        dialogueOpenedFrame = Time.frameCount;

        waitingForCloseClick = false;
        choiceSelectedFrame = -1;

        dialoguePanel.SetActive(true);

        LoadDialogue(data);
    }

    private void LoadDialogue(DialogueData data)
    {
        if (data == null)
            return;

        currentDialogue = data;

        waitingForCloseClick = false;
        choiceSelectedFrame = -1;
        dialogueOpenedFrame = Time.frameCount;

        currentLineIndex = 0;

        ClearChoices();

        // ถ้ามี Conversation Lines ให้ใช้ระบบต่อเนื่อง
        isSequenceDialogue =
            currentDialogue.conversationLines != null &&
            currentDialogue.conversationLines.Length > 0;

        if (isSequenceDialogue)
        {
            ShowSequenceLine();
        }
        else
        {
            // ถ้าไม่มี Conversation Lines ให้ใช้ระบบเดิม
            ShowPlayerDialogue();
        }
    }

    // =========================================
    // ระบบบทสนทนาต่อเนื่องแบบไม่มีตัวเลือก
    // =========================================

    private void ShowSequenceLine()
    {
        if (currentDialogue == null ||
            currentDialogue.conversationLines == null)
        {
            EndDialogue();
            return;
        }

        // ข้ามบรรทัดที่ไม่ได้กำหนดข้อมูล
        while (currentLineIndex <
               currentDialogue.conversationLines.Length &&
               currentDialogue.conversationLines[currentLineIndex] == null)
        {
            currentLineIndex++;
        }

        if (currentLineIndex >=
            currentDialogue.conversationLines.Length)
        {
            EndDialogue();
            return;
        }

        DialogueLine line =
            currentDialogue.conversationLines[currentLineIndex];

        dialogueStage = DialogueStage.SequenceLine;

        if (npcNameText != null)
        {
            string speaker;

            // บรรทัดแรกใช้ชื่อจาก Player Name
            if (currentLineIndex == 0)
            {
                speaker = currentDialogue.playerName;

                // ถ้า Player Name ว่าง ให้ใช้ Speaker Name
                if (string.IsNullOrWhiteSpace(speaker))
                    speaker = line.speakerName;
            }
            else
            {
                // บรรทัดถัดไปใช้ Speaker Name
                speaker = line.speakerName;
            }

            // ถ้าไม่มีชื่อ ให้ใช้ชื่อ NPC
            if (string.IsNullOrWhiteSpace(speaker))
                speaker = currentDialogue.npcName;

            npcNameText.text = speaker;
        }

        if (dialogueText != null)
            dialogueText.text = line.dialogueText;
    }

    private void AdvanceSequenceDialogue()
    {
        if (currentDialogue == null ||
            currentDialogue.conversationLines == null)
        {
            EndDialogue();
            return;
        }

        currentLineIndex++;

        if (currentLineIndex <
            currentDialogue.conversationLines.Length)
        {
            ShowSequenceLine();
        }
        else
        {
            // อ่านครบทุกบรรทัดแล้วปิด Panel
            EndDialogue();
        }
    }

    // =========================================
    // ระบบบทสนทนาเดิม
    // =========================================

    private void ShowPlayerDialogue()
    {
        if (currentDialogue == null)
            return;

        waitingForCloseClick = false;
        dialogueStage = DialogueStage.PlayerLine;

        ClearChoices();

        if (npcNameText != null)
        {
            npcNameText.text =
                string.IsNullOrWhiteSpace(currentDialogue.playerName)
                ? "Player"
                : currentDialogue.playerName;
        }

        if (dialogueText != null)
            dialogueText.text = currentDialogue.playerDialogueText;

        // ถ้าไม่มีข้อความ Player ให้เริ่มที่ NPC
        if (string.IsNullOrWhiteSpace(
            currentDialogue.playerDialogueText))
        {
            ShowNpcDialogue();
        }
    }

    private void ShowNpcDialogue()
    {
        if (currentDialogue == null)
            return;

        dialogueStage = DialogueStage.NpcLine;

        if (npcNameText != null)
            npcNameText.text = currentDialogue.npcName;

        if (dialogueText != null)
            dialogueText.text = currentDialogue.dialogueText;

        ShowChoices();
    }

    private void AdvanceDialogue()
    {
        if (currentDialogue == null)
            return;

        switch (dialogueStage)
        {
            case DialogueStage.PlayerLine:

                // แสดงบทพูด NPC หลังจาก Player พูด
                ShowNpcDialogue();
                break;

            case DialogueStage.NpcLine:

                // ถ้าไม่มีตัวเลือก คลิกอีกครั้งเพื่อปิด
                if (currentDialogue.choices == null ||
                    currentDialogue.choices.Length == 0)
                {
                    EndDialogue();
                }

                break;

            case DialogueStage.ChoiceResponse:

                // การคลิกปิดถูกจัดการใน Update()
                break;
        }
    }

    // =========================================
    // ระบบตัวเลือก
    // =========================================

    private void ShowChoices()
    {
        ClearChoices();

        if (currentDialogue == null ||
            choicesParent == null ||
            choiceButtonPrefab == null ||
            currentDialogue.choices == null)
        {
            return;
        }

        foreach (DialogueChoice choice in currentDialogue.choices)
        {
            if (choice == null)
                continue;

            DialogueChoice selectedChoice = choice;

            Button button = Instantiate(
                choiceButtonPrefab,
                choicesParent
            );

            TMP_Text buttonText =
                button.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
                buttonText.text = selectedChoice.choiceText;

            button.onClick.AddListener(() =>
            {
                SelectChoice(selectedChoice);
            });
        }
    }

    private void SelectChoice(DialogueChoice choice)
    {
        if (choice == null || currentDialogue == null)
            return;

        // ป้องกันการกดตัวเลือกซ้ำ
        ClearChoices();

        // ถ้าติ๊ก End Dialogue ให้ปิดทันที
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

        // ไม่มีบทสนทนาถัดไป:
        // แสดงคำตอบและรอคลิกเมาส์เพื่อปิด
        waitingForCloseClick = true;
        choiceSelectedFrame = Time.frameCount;

        dialogueStage = DialogueStage.ChoiceResponse;

        if (npcNameText != null)
            npcNameText.text = currentDialogue.npcName;

        if (dialogueText != null)
            dialogueText.text = choice.responseText;
    }

    private void ClearChoices()
    {
        if (choicesParent == null)
            return;

        foreach (Transform child in choicesParent)
        {
            Button button = child.GetComponent<Button>();

            if (button != null)
                button.interactable = false;

            Destroy(child.gameObject);
        }
    }

    // =========================================
    // จบบทสนทนา
    // =========================================

    public void EndDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        ClearChoices();

        currentDialogue = null;

        isSequenceDialogue = false;
        currentLineIndex = 0;

        waitingForCloseClick = false;
        choiceSelectedFrame = -1;
        dialogueOpenedFrame = -1;
        waitForInputRelease = false;

        // คืนสถานะการเดินเดิม
        if (playerMovement != null && hasSavedMovementState)
        {
            playerMovement.enabled = previousMovementEnabled;
        }

        hasSavedMovementState = false;

        // คืนสถานะเมาส์เดิม
        Cursor.lockState = previousCursorLockMode;
        Cursor.visible = previousCursorVisible;
    }
}
