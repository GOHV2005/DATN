// QuestUIManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestUIManager : MonoBehaviour
{
    public static QuestUIManager Instance;

    [Header("=== UI References ===")]
    public GameObject questPanel;
    public GameObject questEntryPrefab;

    private Dictionary<QuestNPC, GameObject> questEntries = new();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (questPanel != null) questPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            questPanel.SetActive(!questPanel.activeSelf);
        }
    }

    public void ShowQuest(QuestNPC npc)
    {
        if (questEntries.ContainsKey(npc)) return;

        GameObject entryObj = Instantiate(questEntryPrefab, questPanel.transform);
        Text text = entryObj.GetComponentInChildren<Text>();
        if (text != null)
        {
            text.text = npc.questDescription;
            text.color = Color.black;
        }
        
        questEntries[npc] = entryObj;
        questPanel.SetActive(true);
        
    }

    public void UpdateQuest(QuestNPC npc)
    {
        if (!questEntries.TryGetValue(npc, out GameObject entryObj))
        {
            Debug.LogWarning($"[QuestUI] Không tìm thấy nhiệm vụ để cập nhật: {npc.name}");
            return;
        }

        Text text = entryObj.GetComponentInChildren<Text>();
        if (text != null)
        {
            text.text = npc.questDescription;
        }
    }

    public void CompleteQuest(QuestNPC npc)
    {
        if (!questEntries.TryGetValue(npc, out GameObject entryObj))
        {
            Debug.LogError($"[QuestUI] Không tìm thấy nhiệm vụ của {npc.name}!");
            return;
        }

        Text text = entryObj.GetComponentInChildren<Text>();
        if (text != null)
        {
            text.text = "Hoàn thành!";
            text.color = Color.green;
        }
        // 👇 TỰ ĐỘNG ẨN SAU 2 GIÂY
        StartCoroutine(RemoveQuestAfterDelay(npc, entryObj, 3f));
    }
    IEnumerator RemoveQuestAfterDelay(QuestNPC npc, GameObject entryObj, float delay)
    {
        yield return new WaitForSeconds(delay);

        Destroy(entryObj);
        questEntries.Remove(npc);

        // 👇 ẨN PANEL NẾU KHÔNG CÒN NHIỆM VỤ
        if (questEntries.Count == 0)
        {
            questPanel.SetActive(false);
        }
    }

    public void HideQuest(QuestNPC npc)
    {
        if (questEntries.TryGetValue(npc, out GameObject entryObj))
        {
            Destroy(entryObj);
            questEntries.Remove(npc);
            if (questEntries.Count == 0)
            {
                questPanel.SetActive(false);
            }
        }
    }
}