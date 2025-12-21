using UnityEngine;

public class QuestNPC : MonoBehaviour
{
    [Header("=== Camera Focus ===")]
    public bool enableCameraFocus = false;
    public Transform cameraFocusTarget;

    [Header("=== Quest Data ===")]
    public string questDescription = "";

    public string[] requiredItems;
    public int[] requiredAmounts;

    [Header("=== Dialogue ===")]
    public Dialogue questDialogue;          // giao quest
    public Dialogue missingItemDialogue;    // thiếu item
    public Dialogue completeDialogue;       // hoàn thành
    public Dialogue comebackDialogue;       // nói chuyện sau khi xong

    [Header("=== NPC Settings ===")]
    public bool enableLookAtPlayer = true;
    public Transform playerTransform;
    public float interactionRange = 2f;
    public GameObject interactionPrompt;

    [Header("=== Reward ===")]
    public GameObject rewardItemPrefab;
    public float dropForce = 8f;
    public float dropAngle = 50f;

    // STATE
    private bool hasAcceptedQuest;
    private bool isQuestCompleted;
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
        // 1️⃣ Chưa nhận quest
        if (!hasAcceptedQuest && !isQuestCompleted)
        {
            if (questDialogue != null)
            {
                DialogueSystem.Instance.StartQuestDialogue(questDialogue, this);
            }
            return;
        }

        // 2️⃣ Đã nhận – chưa xong → kiểm tra item
        if (hasAcceptedQuest && !isQuestCompleted)
        {
            TryCompleteQuest();
            return;
        }

        // 3️⃣ Đã hoàn thành → dialogue quay lại
        if (isQuestCompleted && comebackDialogue != null)
        {
            DialogueSystem.Instance.StartDialogue(comebackDialogue);
        }
    }

    void LookAtPlayer()
    {
        if (!enableLookAtPlayer || sr == null) return;

        sr.flipX = playerTransform.position.x < transform.position.x;
    }

    // === ĐƯỢC GỌI TỪ DialogueSystem SAU KHI HỘI THOẠI GIAO QUEST KẾT THÚC ===
    public void OnAcceptQuest()
    {
        if (hasAcceptedQuest || isQuestCompleted) return;

        hasAcceptedQuest = true;

        QuestUIManager.Instance?.ShowQuest(this);
        QuestlineManager.Instance?.OnAcceptNPCQuest(gameObject.name);
    }

    void TryCompleteQuest()
    {
        InventoryManager inv = InventoryManager.Instance;
        if (inv == null) return;

        int count = requiredItems.Length;

        for (int i = 0; i < count; i++)
        {
            int need = (requiredAmounts != null && i < requiredAmounts.Length)
                ? Mathf.Max(1, requiredAmounts[i])
                : 1;

            if (inv.GetItemCount(requiredItems[i]) < need)
            {
                // ❌ Thiếu item
                if (missingItemDialogue != null)
                    DialogueSystem.Instance.StartDialogue(missingItemDialogue);
                return;
            }
        }

        // ✅ ĐỦ ITEM → TRỪ
        for (int i = 0; i < count; i++)
        {
            int need = (requiredAmounts != null && i < requiredAmounts.Length)
                ? Mathf.Max(1, requiredAmounts[i])
                : 1;

            inv.RemoveItem(requiredItems[i], need);
        }

        CompleteQuest();
    }

    void CompleteQuest()
    {
        hasAcceptedQuest = false;
        isQuestCompleted = true;

        QuestUIManager.Instance?.CompleteQuest(this);
        QuestlineManager.Instance?.OnCompleteNPCQuest(gameObject.name);

        if (completeDialogue != null)
            DialogueSystem.Instance.StartDialogue(completeDialogue);

        DropRewardItem();
    }

    void DropRewardItem()
    {
        if (rewardItemPrefab == null || playerTransform == null) return;

        GameObject item = Instantiate(rewardItemPrefab, transform.position, Quaternion.identity);
        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            Vector2 dir = (playerTransform.position - transform.position).normalized;
            float rad = Mathf.Deg2Rad * dropAngle;

            Vector2 force = new Vector2(
                Mathf.Cos(rad) * dir.x,
                Mathf.Sin(rad)
            ) * dropForce;

            rb.AddForce(force, ForceMode2D.Impulse);
        }
    }

    void ShowInteractionPrompt()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(true);
    }

    void HideInteractionPrompt()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }
}
