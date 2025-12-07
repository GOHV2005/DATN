// DialogueSystem.cs
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
    public Button btnYes;
    public Button btnNo;
    public GameObject dialoguePanel;

    private Dialogue currentDialogue;
    private QuestNPC currentNPC;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool showChoice = false;
    // Thêm vào đầu class DialogueSystem
    private Coroutine typeTextCoroutine;
    private Coroutine typeTextWithChoiceCoroutine;
    // Trong DialogueSystem.cs
    private bool isDialogueFinished = false;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        dialoguePanel.SetActive(false);
        btnYes.gameObject.SetActive(false);
        btnNo.gameObject.SetActive(false);
    }

    void Update()
    {
        // 👇 NHẤN ESC ĐỂ TẮT
        if (Input.GetKeyDown(KeyCode.Escape) && dialoguePanel.activeSelf)
        {
            CloseDialogue();
            return;
        }

        if (dialoguePanel.activeSelf && isDialogueFinished && Input.GetMouseButtonDown(0))
        {
            CloseDialogue();
        }
    }

    public void StartQuestDialogue(Dialogue dialogue, QuestNPC npc)
    {
        isDialogueFinished = false;
        currentDialogue = dialogue;
        currentNPC = npc;
        currentLineIndex = 0;
        showChoice = false;
        dialoguePanel.SetActive(true);
        btnYes.gameObject.SetActive(false);
        btnNo.gameObject.SetActive(false);
        // 👇 GÁN SPRITE
        if (dialogue.playerSprite != null) playerPortrait.sprite = dialogue.playerSprite;
        if (dialogue.npcSprite != null) npcPortrait.sprite = dialogue.npcSprite;
        UIManager.IsTalkingToNPC = true;
        DisplayNextLine();
    }

    public void StartSingleLine(DialogueLine line, Sprite playerSprite, Sprite npcSprite)
    {
        var tempDialogue = new Dialogue
        {
            playerSprite = playerSprite,
            npcSprite = npcSprite,
            lines = new List<DialogueLine> { line } // 👈 SỬA DÒNG NÀY
        };
        StartDialogue(tempDialogue);
    }

    public void StartDialogue(Dialogue dialogue)
    {
        isDialogueFinished = false;
        currentDialogue = dialogue;
        currentNPC = null;
        currentLineIndex = 0;
        showChoice = false;
        dialoguePanel.SetActive(true);
        btnYes.gameObject.SetActive(false);
        btnNo.gameObject.SetActive(false);

        // 👇 GÁN SPRITE
        if (dialogue.playerSprite != null) playerPortrait.sprite = dialogue.playerSprite;
        if (dialogue.npcSprite != null) npcPortrait.sprite = dialogue.npcSprite;
        UIManager.IsTalkingToNPC = true;
        DisplayNextLine();
    }

    void DisplayNextLine()
    {
        if (currentLineIndex >= currentDialogue.lines.Count)
        {
            return;
        }

        var line = currentDialogue.lines[currentLineIndex];
        UpdatePortraits(line.isPlayer);

        if (line.isQuestRequest && currentNPC != null)
        {
            StartCoroutine(TypeTextWithChoice(line.text));
        }
        else
        {
            StartCoroutine(TypeText(line.text));
        }
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in text)
        {
            if (currentDialogue == null) yield break; // 👈 THÊM DÒNG NÀY
            dialogueText.text += c;
            yield return new WaitForSeconds(0.03f);
            if (Input.GetMouseButtonDown(0))
            {
                dialogueText.text = text;
                break;
            }
        }

        if (currentDialogue == null) yield break; // 👈 THÊM DÒNG NÀY

        isTyping = false;

        // Chờ click
        while (!Input.GetMouseButtonDown(0))
        {
            if (currentDialogue == null) yield break; // 👈 THÊM DÒNG NÀY
            yield return null;
        }

        if (currentDialogue == null) yield break; // 👈 THÊM DÒNG NÀY

        currentLineIndex++;
        if (currentLineIndex < currentDialogue.lines.Count)
        {
            DisplayNextLine();
        }
        else
        {
            isDialogueFinished = true; // 👈 HẾT TOÀN BỘ HỘI THOẠI
        }
    }

    IEnumerator TypeTextWithChoice(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in text)
        {
            if (currentDialogue == null) yield break;
            dialogueText.text += c;
            yield return new WaitForSeconds(0.03f);
            if (Input.GetMouseButtonDown(0))
            {
                dialogueText.text = text;
                break;
            }
        }

        if (currentDialogue == null) yield break;

        isTyping = false;

        while (!Input.GetMouseButtonDown(0))
        {
            if (currentDialogue == null) yield break;
            yield return null;
        }

        if (currentDialogue == null) yield break;

        ShowChoiceButtons();
    }

    void ShowChoiceButtons()
    {
        showChoice = true;
        btnYes.gameObject.SetActive(true);
        btnNo.gameObject.SetActive(true);

        btnYes.onClick.RemoveAllListeners();
        btnNo.onClick.RemoveAllListeners();

        btnYes.onClick.AddListener(() =>
        {
            btnYes.gameObject.SetActive(false);
            btnNo.gameObject.SetActive(false);
            showChoice = false;
            if (currentNPC != null) currentNPC.OnAcceptQuest();
        });

        btnNo.onClick.AddListener(() =>
        {
            btnYes.gameObject.SetActive(false);
            btnNo.gameObject.SetActive(false);
            showChoice = false;
            if (currentNPC != null) currentNPC.OnRefuseQuest();
        });
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
        currentDialogue = null; // 👈 ĐẶT NGAY ĐẦU
        currentNPC = null;
        currentLineIndex = 0;
        showChoice = false;
        btnYes.gameObject.SetActive(false);
        btnNo.gameObject.SetActive(false);

        // 👇 HỦY COROUTINE NẾU CÓ (TÙY CHỌN)
        StopAllCoroutines();
    }
}