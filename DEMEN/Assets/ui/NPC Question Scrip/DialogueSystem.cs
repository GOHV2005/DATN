using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem Instance;

    [Header("UI References")]
    public Image playerPortrait;
    public Image npcPortrait;
    public Text dialogueText;
    public GameObject dialoguePanel;

    private Dialogue currentDialogue;
    private QuestNPC currentNPC; // nếu đang là hội thoại quest thì giữ tham chiếu
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool isDialogueFinished = false;
    public System.Action OnDialogueComplete;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        dialoguePanel.SetActive(false);
        UIManager.IsTalkingToNPC = false;
    }

    void Update()
    {
        // ESC tắt dialog
        if (Input.GetKeyDown(KeyCode.Escape) && dialoguePanel.activeSelf)
        {
            CloseDialogue();
            return;
        }
        // Nếu dialog đang hiển thị, đã xong và không phải quest
        if (dialoguePanel.activeSelf && isDialogueFinished && currentNPC == null)
        {
            CloseDialogue();
        }

        // Nếu đang hiển thị dialog và đã hoàn tất text (nhưng chưa xử lý accept)
        if (dialoguePanel.activeSelf && isDialogueFinished && currentNPC != null)
        {
            // Tự động nhận quest khi dialog của NPC kết thúc
            currentNPC.OnAcceptQuest();
            // Ngắt tham chiếu để không gọi lại
            currentNPC = null;
            // (Dialog panel giữ nguyên — OnAcceptQuest sẽ show quest UI)
        }

        // Nếu dialog đang hiển thị và đã hoàn tất (không phải quest)
        if (dialoguePanel.activeSelf && isDialogueFinished && Input.GetMouseButtonDown(0) && currentNPC == null)
        {
            CloseDialogue();
        }
    }

    // Bắt đầu hội thoại bình thường
    public void StartDialogue(Dialogue dialogue)
    {
        UIManager.IsTalkingToNPC = true;
        isDialogueFinished = false;
        currentDialogue = dialogue;
        currentLineIndex = 0;
        dialoguePanel.SetActive(true);
        if (dialogue.playerSprite != null) playerPortrait.sprite = dialogue.playerSprite;
        if (dialogue.npcSprite != null) npcPortrait.sprite = dialogue.npcSprite;
        DisplayNextLine();
    }

    // Bắt đầu hội thoại quest: set currentNPC để tự động accept sau khi dialog kết thúc
    public void StartQuestDialogue(Dialogue dialogue, QuestNPC npc)
    {
        currentNPC = npc;
        StartDialogue(dialogue);
    }

    void DisplayNextLine()
    {
        if (currentDialogue == null || currentLineIndex >= currentDialogue.lines.Count)
        {
            isDialogueFinished = true;
            return;
        }

        var line = currentDialogue.lines[currentLineIndex];
        UpdatePortraits(line.isPlayer);
        StartCoroutine(TypeText(line.text));
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.03f);
            if (Input.GetMouseButtonDown(0))
            {
                dialogueText.text = text;
                break;
            }
        }

        isTyping = false;

        // Chờ click để chuyển câu
        while (!Input.GetMouseButtonDown(0))
        {
            yield return null;
        }

        currentLineIndex++;
        if (currentLineIndex < currentDialogue.lines.Count)
        {
            DisplayNextLine();
        }
        else
        {
            isDialogueFinished = true;
            OnDialogueComplete?.Invoke();
        }
        
    }

    void UpdatePortraits(bool isPlayerSpeaking)
    {
        if (isPlayerSpeaking)
        {
            playerPortrait.color = Color.white;
            npcPortrait.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        }
        else
        {
            npcPortrait.color = Color.white;
            playerPortrait.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        }
    }

    public void CloseDialogue()
    {
        UIManager.IsTalkingToNPC = false;
        dialoguePanel.SetActive(false);
        if (!isDialogueFinished)
            OnDialogueComplete?.Invoke();
        currentDialogue = null;
        currentLineIndex = 0;
        isTyping = false;
        isDialogueFinished = false;
        currentNPC = null;
        StopAllCoroutines();
    }

    // Hỗ trợ gọi 1 dòng nhanh
    public void StartSingleLine(DialogueLine line, Sprite playerSprite, Sprite npcSprite)
    {
        var tmp = new Dialogue
        {
            playerSprite = playerSprite,
            npcSprite = npcSprite,
            lines = new List<DialogueLine> { line }
        };
        StartDialogue(tmp);
    }
}
