using UnityEngine;

[System.Serializable]
public class DialogueChoice
{
    public string choiceText;
    public int nextNodeIndex; // -1 means end
    // Optional: add fields later like condition flags or event keys
}

[System.Serializable]
public class DialogueNode
{
    [TextArea(2, 5)] public string npcLine;
    public DialogueChoice[] playerChoices;
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Conversation")]
public class Dialogue : ScriptableObject
{
    public DialogueNode[] nodes;
}
