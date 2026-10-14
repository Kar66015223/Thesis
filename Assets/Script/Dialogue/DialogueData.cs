
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewDialogue",
    menuName = "Dialogue/Dialogue Data"
)]
public class DialogueData : ScriptableObject
{
    [Header("Sequential Conversation - No Choices")]
    public DialogueLine[] conversationLines;

    [Header("Player Information")]
    public string playerName = "Player";

    [TextArea(3, 6)]
    public string playerDialogueText;

    [Header("NPC Information")]
    public string npcName;

    [TextArea(3, 6)]
    public string dialogueText;

    [Header("Player Choices")]
    public DialogueChoice[] choices;
}

[System.Serializable]
public class DialogueLine
{
    [Header("Speaker")]
    public string speakerName;

    [TextArea(3, 6)]
    public string dialogueText;
}

[System.Serializable]
public class DialogueChoice
{
    [Header("Choice")]
    public string choiceText;

    [TextArea(2, 4)]
    public string responseText;

    [Header("Dialogue Flow")]
    public bool endDialogue;

    public DialogueData nextDialogue;
}
