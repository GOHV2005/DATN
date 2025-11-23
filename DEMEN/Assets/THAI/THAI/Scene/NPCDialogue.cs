using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    public string[] sentences;      // Các đoạn hội thoại
    public bool playerInRange;

    public DialogueManager dialogueManager;

    private void Start()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            dialogueManager.StartDialogue(sentences);
        }
    }
}
