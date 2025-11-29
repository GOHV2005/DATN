// QuestNPC.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class QuestNPC : MonoBehaviour
{
    public string[] requiredItems;
    private List<string> collectedItems = new();
    private bool isQuestCompleted = false;
    private bool hasAcceptedQuest = false;
    [Header("=== Look at Player ===")]
    public bool enableLookAtPlayer = true; // 👈 MỚI: bật/tắt nhìn theo player
    public Dialogue fullDialogue;

    public Transform playerTransform;
    public float interactionRange = 2f;
    [Header("=== Thông tin nhiệm vụ ===")]
    public string questDescription = "";//Mô tả nhiệm vụ...

    [Header("=== Ném phần thưởng ===")]
    public GameObject rewardItemPrefab;
    public float dropForce = 8f;
    public float dropAngle = 50f;
    private bool canReceiveItems = false; // 👈 MỚI: chỉ nhận item khi true

    public GameObject interactionPrompt;
    private SpriteRenderer sr; // 👈 THÊM
    
    void Start()
    {
        sr = GetComponent<SpriteRenderer>(); // 👈 LẤY SPRITE RENDERER
        HideInteractionPrompt();
    }

    void Update()
    {
        if (playerTransform == null) return;

        LookAtPlayer();

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist <= interactionRange && UIManager.IsGameplayInputAllowed)
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
        if (isQuestCompleted)
        {
            var comebackLine = fullDialogue.lines.Find(l => l.isDoneIfPlayerComeBack);
            if (comebackLine != null)
            {
                DialogueSystem.Instance.StartSingleLine(comebackLine, fullDialogue.playerSprite, fullDialogue.npcSprite);
            }
        }
        else if (hasAcceptedQuest)
        {
            OnShowQuestComplete();
        }
        else
        {
            DialogueSystem.Instance.StartQuestDialogue(fullDialogue, this);
        }
    }
    void LookAtPlayer()
    {
        if (!enableLookAtPlayer || playerTransform == null || sr == null) return;

        // Lật sprite theo hướng player
        if (playerTransform.position.x > transform.position.x)
        {
            sr.flipX = false; // Nhìn phải
        }
        else
        {
            sr.flipX = true; // Nhìn trái
        }
    }
    public void OnAcceptQuest()
    {
        hasAcceptedQuest = true;
        canReceiveItems = true;
        enableLookAtPlayer = false;
        // 👇 HIỆN PANEL NHIỆM VỤ
        QuestUIManager.Instance.ShowQuest(this);

        var acceptLine = fullDialogue.lines.Find(l => l.isAcceptLine);
        if (acceptLine != null)
        {
            DialogueSystem.Instance.StartSingleLine(acceptLine, fullDialogue.playerSprite, fullDialogue.npcSprite);
        }
    }

    public void OnRefuseQuest()
    {
        var refuseLine = fullDialogue.lines.Find(l => l.isRefuseLine);
        if (refuseLine != null)
        {
            DialogueSystem.Instance.StartSingleLine(refuseLine, fullDialogue.playerSprite, fullDialogue.npcSprite);
        }
        hasAcceptedQuest = false;
        canReceiveItems = false;
         // Ẩn nhiệm vụ nếu từ chối
    }

    void OnShowQuestComplete()
    {
        bool hasAll = collectedItems.Count == requiredItems.Length;
        if (hasAll)
        {
            var completeLine = fullDialogue.lines.Find(l => l.isQuestComplete);
            if (completeLine != null)
            {
                DialogueSystem.Instance.StartSingleLine(completeLine, fullDialogue.playerSprite, fullDialogue.npcSprite);
                isQuestCompleted = true;
                canReceiveItems = false;
                QuestUIManager.Instance.CompleteQuest(this); // Đánh dấu hoàn thành

                if (rewardItemPrefab != null && playerTransform != null)
                {
                    Invoke(nameof(DropRewardItem), 1f);
                }
            }
        }
        else
        {
            var missingLine = fullDialogue.lines.Find(l => l.isMissingItem);
            if (missingLine != null)
            {
                DialogueSystem.Instance.StartSingleLine(missingLine, fullDialogue.playerSprite, fullDialogue.npcSprite);
            }
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

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!canReceiveItems || isQuestCompleted) return;
        if (col.collider.TryGetComponent(out Item item))
        {
            if (requiredItems.Contains(item.itemName) && !collectedItems.Contains(item.itemName))
            {
                collectedItems.Add(item.itemName);
                Destroy(col.gameObject);
                QuestUIManager.Instance.UpdateQuest(this);
            }
        }
    }
    void OnTriggerEnter2D(Collider2D col)
    {
        if (!canReceiveItems || isQuestCompleted) return;
        if (col.TryGetComponent(out Item item))
        {
            if (requiredItems.Contains(item.itemName) && !collectedItems.Contains(item.itemName))
            {
                collectedItems.Add(item.itemName);
                Destroy(col.gameObject);
                QuestUIManager.Instance.UpdateQuest(this);
            }
        }
    }
    void ShowInteractionPrompt()
    {
        if (interactionPrompt != null) interactionPrompt.SetActive(true);
    }

    void HideInteractionPrompt()
    {
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
    }
}