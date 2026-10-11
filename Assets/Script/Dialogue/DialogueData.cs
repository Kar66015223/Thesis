
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewDialogue",
    menuName = "Dialogue/Dialogue Data"
)]
public class DialogueData : ScriptableObject
{
    [Header("NPC Information")]
    public string npcName;

    [TextArea(3, 6)]
    public string dialogueText;

    [Header("Player Choices")]
    public DialogueChoice[] choices;
}

[System.Serializable]
public class DialogueChoice
{
    public string choiceText;

    [TextArea(2, 4)]
    public string responseText;
}
