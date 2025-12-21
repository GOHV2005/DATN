using UnityEngine;
using System.Collections.Generic;

public class QuestNPC : MonoBehaviour
{

    [Header("=== Quest Data ===")]
    public string questDescription = "";
    public string[] requiredItems;         // tên item cần nộp
    public int[] requiredAmounts;          // số lượng tương ứng (nếu thiếu, mặc định 1)
    public Dialogue fullDialogue;          // hội thoại khi giao quest
    public Dialogue missingItemLineDialogue; // optional: line when missing
    public Dialogue completeLineDialogue;    // optional: line when complete
    public Dialogue comebackLineDialogue;    // optional: line when player returns after completion

    [Header("=== NPC Settings ===")]
    public bool enableLookAtPlayer = true;
    public Transform playerTransform;
    public float interactionRange = 2f;
    public GameObject interactionPrompt;
    public GameObject rewardItemPrefab;
    public float dropForce = 8f;
    public float dropAngle = 50f;

    // state
    private bool hasAcceptedQuest = false;
    private bool isQuestCompleted = false;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        HideInteractionPrompt();
    }

    void Update()
    {
        if (playerTransform == null) return;

        LookAtPlayer();

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist <= interactionRange && !UIManager.IsTalkingToNPC)
        {
            ShowInteractionPrompt();
            if (Input.GetKeyDown(KeyCode.E))
            {
                HideInteractionPrompt();
                HandleInteraction();
            }
        }
        else
        {
            HideInteractionPrompt();
        }
    }

    void HandleInteraction()
    {
        if (!hasAcceptedQuest && !isQuestCompleted)
        {
            // bắt đầu hội thoại giao nhiệm vụ → sau khi hội thoại kết thúc DialogueSystem sẽ gọi OnAcceptQuest()
            if (fullDialogue != null)
                DialogueSystem.Instance.StartQuestDialogue(fullDialogue, this);
        }
        else if (hasAcceptedQuest && !isQuestCompleted)
        {
            // player tới trả quest: kiểm tra inventory
            OnShowQuestComplete();
        }
        else if (isQuestCompleted)
        {
            // đã hoàn thành, player quay lại
            if (comebackLineDialogue != null)
                DialogueSystem.Instance.StartSingleLine(comebackLineDialogue.lines.Count > 0 ? comebackLineDialogue.lines[0] : new DialogueLine { text = "Cảm ơn!" }, fullDialogue.playerSprite, fullDialogue.npcSprite);
        }
    }

    void LookAtPlayer()
    {
        if (!enableLookAtPlayer || playerTransform == null || sr == null) return;

        if (playerTransform.position.x > transform.position.x)
            sr.flipX = false;
        else
            sr.flipX = true;
    }

    // Gọi tự động khi DialogueSystem kết thúc hội thoại quest
    public void OnAcceptQuest()
    {
        if (hasAcceptedQuest || isQuestCompleted) return;

        hasAcceptedQuest = true;
        // show in quest UI
        QuestUIManager.Instance?.ShowQuest(this);
        QuestlineManager.Instance?.OnAcceptNPCQuest(gameObject.name);
        // Optionally show a short accept line if defined
        var acceptLine = fullDialogue.lines.Find(l => l.isAcceptLine);
        if (acceptLine != null)
        {
            DialogueSystem.Instance.StartSingleLine(acceptLine, fullDialogue.playerSprite, fullDialogue.npcSprite);
        }
    }

    // Kiểm tra inventory khi player trả quest
    public void OnShowQuestComplete()
    {
        if (!hasAcceptedQuest || isQuestCompleted) return;
        QuestlineManager.Instance?.OnCompleteNPCQuest(gameObject.name);
        UIManager.IsTalkingToNPC = false;
        InventoryManager inv = InventoryManager.Instance;
        if (inv == null)
        {
            Debug.LogWarning("[QuestNPC] InventoryManager not found");
            return;
        }

        // chuẩn hóa length của requiredAmounts
        int reqCount = requiredItems != null ? requiredItems.Length : 0;
        int[] amounts = new int[reqCount];
        for (int i = 0; i < reqCount; i++)
            amounts[i] = (requiredAmounts != null && i < requiredAmounts.Length) ? Mathf.Max(1, requiredAmounts[i]) : 1;

        // kiểm tra đủ
        bool allEnough = true;
        for (int i = 0; i < reqCount; i++)
        {
            string name = requiredItems[i];
            int need = amounts[i];
            int have = inv.GetItemCount(name);
            if (have < need)
            {
                allEnough = false;
                break;
            }
        }

        if (!allEnough)
        {
            // thiếu item → show missing line
            var missingLine = fullDialogue.lines.Find(l => l.isMissingItem);
            if (missingLine != null)
                DialogueSystem.Instance.StartSingleLine(missingLine, fullDialogue.playerSprite, fullDialogue.npcSprite);
            else if (missingItemLineDialogue != null)
                DialogueSystem.Instance.StartDialogue(missingItemLineDialogue);
            return;
        }

        // đủ → trừ item và hoàn thành quest
        for (int i = 0; i < reqCount; i++)
        {
            string name = requiredItems[i];
            int need = amounts[i];
            inv.RemoveItem(name, need);
        }

        isQuestCompleted = true;
        hasAcceptedQuest = false; // không còn chấp nhận nữa

        // Update UI
        QuestUIManager.Instance?.CompleteQuest(this);

        // show complete line
        var completeLine = fullDialogue.lines.Find(l => l.isQuestComplete);
        if (completeLine != null)
            DialogueSystem.Instance.StartSingleLine(completeLine, fullDialogue.playerSprite, fullDialogue.npcSprite);
        else if (completeLineDialogue != null)
            DialogueSystem.Instance.StartDialogue(completeLineDialogue);

        // drop reward
        if (rewardItemPrefab != null && playerTransform != null)
        {
            DropRewardItem();
        }
    }

    void DropRewardItem()
    {
        if (rewardItemPrefab == null || playerTransform == null) return;

        GameObject itemObj = Instantiate(rewardItemPrefab, transform.position, Quaternion.identity);
        Rigidbody2D rb = itemObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 3f;

            Vector2 direction = (playerTransform.position - transform.position).normalized;
            float rad = Mathf.Deg2Rad * dropAngle;
            float vx = Mathf.Cos(rad) * direction.x;
            float vy = Mathf.Sin(rad); // Luôn hướng lên

            Vector2 force = new Vector2(vx, vy) * dropForce;
            rb.AddForce(force, ForceMode2D.Impulse);
        }
    }

    // UI interaction prompt
    void ShowInteractionPrompt() { if (interactionPrompt != null) interactionPrompt.SetActive(true); }
    void HideInteractionPrompt() { if (interactionPrompt != null) interactionPrompt.SetActive(false); }
}
