using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class QuestlineManager : MonoBehaviour
{
    public static QuestlineManager Instance;

    [Header("Questline UI")]
    public GameObject questlinePanel;
    public Text questlineText;

    [Header("Quest Steps")]
    public List<QuestStep> steps = new();

    private int currentStepIndex = 0;
    private QuestStep Current => steps[currentStepIndex];

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        questlinePanel.SetActive(true);
        UpdateUI();
    }

    void Update()
    {
        if (Current.stepType == QuestStepType.GoToPosition)
        {
            float dist = Vector2.Distance(
                PlayerController.Instance.transform.position,
                Current.targetPosition.position);

            if (dist <= Current.reachDistance)
                CompleteStep();
        }
    }

    void UpdateUI()
    {
        questlineText.text = $"▶ {Current.description}";
    }

    void CompleteStep()
    {
        currentStepIndex++;

        if (currentStepIndex >= steps.Count)
        {
            questlineText.text = "🎉 Hoàn thành hành trình!";
            return;
        }

        UpdateUI();
    }

    // ===== EVENT API =====
    public void OnItemPicked(string itemName)
    {
        if (Current.stepType == QuestStepType.PickupItem &&
            Current.targetId == itemName)
            CompleteStep();
    }

    public void OnAcceptNPCQuest(string npcName)
    {
        if (Current.stepType == QuestStepType.AcceptNPCQuest &&
            Current.targetId == npcName)
            CompleteStep();
    }

    public void OnEnemyKilled(string enemyId)
    {
        if (Current.stepType == QuestStepType.KillEnemy &&
            Current.targetId == enemyId)
            CompleteStep();
    }

    public void OnCompleteNPCQuest(string npcName)
    {
        if (Current.stepType == QuestStepType.CompleteNPCQuest &&
            Current.targetId == npcName)
            CompleteStep();
    }
}
