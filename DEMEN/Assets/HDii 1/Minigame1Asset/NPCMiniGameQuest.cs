using UnityEngine;
using TMPro;

public class NPCMiniGameQuest : MonoBehaviour
{
    [Header("Player & Interaction")]
    public Transform player;
    public float interactDistance = 2f;

    [Header("UI")]
    public GameObject pressEUIObject;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public GameObject choicePanel;

    [TextArea]
    public string[] dialogueLines;

    [Header("Minigame")]
    public MinigameController minigameController;

    private int dialogueIndex = 0;
    private bool isTalking = false;

    void Start()
    {
        pressEUIObject.SetActive(false);
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);
    }

    void Update()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= interactDistance)
        {
            if (!isTalking)
                pressEUIObject.SetActive(true);

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (!isTalking)
                    StartDialogue();
                else
                    NextDialogue();
            }
        }
        else
        {
            pressEUIObject.SetActive(false);
        }
    }

    void StartDialogue()
    {
        isTalking = true;
        dialogueIndex = 0;

        pressEUIObject.SetActive(false);
        dialoguePanel.SetActive(true);
        choicePanel.SetActive(false);
        dialogueText.text = dialogueLines[dialogueIndex];
    }

    void NextDialogue()
    {
        dialogueIndex++;

        if (dialogueIndex < dialogueLines.Length)
        {
            dialogueText.text = dialogueLines[dialogueIndex];
        }
        else
        {
            choicePanel.SetActive(true);
        }
    }

    public void OnAccept()
    {
        EndDialogue();
        minigameController.StartMinigame();
    }

    public void OnRefuse()
    {
        EndDialogue();
    }

    void EndDialogue()
    {
        isTalking = false;
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);
    }
}
