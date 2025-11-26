using UnityEngine;

public class NPCInteractor : MonoBehaviour
{
    [Header("Dialogue")]
    public Dialogue dialogue;              // Assign the asset for this NPC
    public DialogueManager dialogueUI;     // Reference to DialogueManager in scene

    private bool playerInRange;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (dialogueUI != null && dialogue != null)
            {
                dialogueUI.StartDialogue(dialogue, 0);
            }
        }
        // Optional: close with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            dialogueUI.EndDialogue();
        }
    }
}
