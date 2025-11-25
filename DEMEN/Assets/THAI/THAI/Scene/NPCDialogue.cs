using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    [Header("NPC Info")]
    public string npcName;
    public Sprite npcPortrait;
    public Sprite playerPortrait;
    public DialogueLine1[] lines;

    private bool playerInRange = false;
    private DialogueManager dialog;

    void Start()
    {
        dialog = FindObjectOfType<DialogueManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            dialog.StartDialogue(npcName, lines, npcPortrait, playerPortrait);
        }
    }
}
