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

    public Dialogue fullDialogue;

    public Transform playerTransform;
    public float interactionRange = 2f;

    [Header("=== Phần thưởng ===")]
    public GameObject rewardItemPrefab;
    public float dropForce = 8f;
    public float dropAngle = 50f;

    void Update()
    {
        if (playerTransform == null) return;
        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist <= interactionRange && Input.GetKeyDown(KeyCode.E))
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
    }

    public void OnAcceptQuest()
    {
        hasAcceptedQuest = true;
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

        // Tạo item
        GameObject itemObj = Instantiate(rewardItemPrefab, transform.position, Quaternion.identity);
        Rigidbody2D rb = itemObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 3f; // Đảm bảo có gravity

            // Tính hướng từ NPC → Player
            Vector2 direction = (playerTransform.position - transform.position).normalized;

            // Tính góc ném (hướng lên)
            float rad = Mathf.Deg2Rad * dropAngle;
            float vx = Mathf.Cos(rad) * direction.x;
            float vy = Mathf.Sin(rad); // Luôn hướng lên

            Vector2 force = new Vector2(vx, vy) * dropForce;
            rb.AddForce(force, ForceMode2D.Impulse);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.TryGetComponent(out Item item))
        {
            if (requiredItems.Contains(item.itemName) && !collectedItems.Contains(item.itemName))
            {
                collectedItems.Add(item.itemName);
                Destroy(col.gameObject);
            }
        }
    }
}