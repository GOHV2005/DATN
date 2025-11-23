using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    private string[] sentences;
    private int index;

    private bool isTalking = false;

    void Start()
    {
        dialoguePanel.SetActive(false);
    }

    public void StartDialogue(string[] newSentences)
    {
        sentences = newSentences;
        index = 0;
        
        dialoguePanel.SetActive(true);
        dialogueText.text = sentences[index];

        isTalking = true;
    }

    void Update()
    {
        if (isTalking && Input.GetMouseButtonDown(0))
        {
            NextSentence();
        }
    }

    void NextSentence()
    {
        index++;

        if (index < sentences.Length)
        {
            dialogueText.text = sentences[index];
        }
        else
        {
            dialoguePanel.SetActive(false);
            isTalking = false;
        }
    }
}
