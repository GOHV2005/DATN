using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel;
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public Image leftPortrait;       // NPC
    public Image rightPortrait;      // Player

    [Header("Typing Settings")]
    private float typingSpeed = 0.03f;

    private DialogueLine1[] lines;
    private int index = 0;
    private bool isTalking = false;
    private bool isTyping = false;

    private string npcName;
    private Sprite npcSprite;
    private Sprite playerSprite;

    void Start()
    {
        panel.SetActive(false);
    }

    public void StartDialogue(string npcName, DialogueLine1[] newLines, Sprite npc, Sprite player)
    {
        Time.timeScale = 0;  // Pause game → player đứng yên

        panel.SetActive(true);

        this.npcName = npcName;
        this.lines = newLines;
        this.npcSprite = npc;
        this.playerSprite = player;

        index = 0;
        isTalking = true;

        ShowSpeaker();
        StartCoroutine(TypeSentence(lines[index].sentence));
    }

    void ShowSpeaker()
    {
        string who = lines[index].speaker;

        // Gán sprite vào portrait
        leftPortrait.sprite = npcSprite;
        rightPortrait.sprite = playerSprite;

        if (who == "NPC")
        {
            // Portrait
            leftPortrait.color = Color.white;                 // NPC sáng
            rightPortrait.color = new Color(1, 1, 1, 0.3f);  // Player mờ

            // NameText và DialogueText
            nameText.text = npcName;
            nameText.color = Color.yellow;                   // NPC nổi bật màu vàng
            dialogueText.color = Color.white;               // chữ luôn sáng
        }
        else // Player nói
        {
            // Portrait
            leftPortrait.color = new Color(1, 1, 1, 0.3f);   // NPC mờ
            rightPortrait.color = Color.white;               // Player sáng

            // NameText và DialogueText
            nameText.text = "Player";
            nameText.color = Color.cyan;                     // Player nổi bật màu xanh
            dialogueText.color = Color.white;               // chữ luôn sáng
        }
    }

    IEnumerator TypeSentence(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;
    }

    void Update()
    {
        if (!isTalking) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = lines[index].sentence;
                isTyping = false;
            }
            else
            {
                NextLine();
            }
        }
    }

    void NextLine()
    {
        index++;

        if (index >= lines.Length)
        {
            EndDialogue();
            return;
        }

        ShowSpeaker();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(lines[index].sentence));
    }

    void EndDialogue()
    {
        panel.SetActive(false);
        isTalking = false;
        Time.timeScale = 1;   // Resume game
    }
}
