using UnityEngine;

/// <summary>
/// เปิดบทสนทนาของ NPC ตัวนี้ (Starts this NPC's conversation).
/// It only raises a request event, so it no longer needs the DialogueManager singleton.
/// </summary>
public class NPCDialogueInteract : Interactable
{
    [Header("NPC Dialogue")]
    [SerializeField] private DialogueData dialogueData;

    protected override void OnInteract(GameObject interactor)
    {
        if (dialogueData != null)
            UIEvents.RequestDialogue(dialogueData);
    }
}
