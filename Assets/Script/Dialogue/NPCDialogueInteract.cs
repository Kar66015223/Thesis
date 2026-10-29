
using UnityEngine;

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
