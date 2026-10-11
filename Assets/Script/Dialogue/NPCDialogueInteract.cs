
using UnityEngine;

public class NPCDialogueInteract : Interactable
{
    [Header("NPC Dialogue")]
    [SerializeField] private DialogueData dialogueData;

    public override void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor))
            return;

        // เรียกการทำงานเดิมของ Interactable
        base.Interact(interactor);

        // เปิดบทสนทนาของ NPC ตัวนี้
        if (DialogueManager.Instance != null &&
            dialogueData != null)
        {
            DialogueManager.Instance.StartDialogue(
                dialogueData
            );
        }
    }
}
