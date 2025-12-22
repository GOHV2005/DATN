using UnityEngine;

public enum QuestStepType
{
    GoToPosition,
    PickupItem,
    AcceptNPCQuest,
    KillEnemy,
    CompleteNPCQuest
}
[System.Serializable]
public class QuestStep
{
    public QuestStepType stepType;
    [TextArea] public string description;

    // dùng cho GoToPosition
    public Transform targetPosition;
    public float reachDistance = 1.5f;

    // dùng cho item / enemy / npc
    public string targetId;
}
