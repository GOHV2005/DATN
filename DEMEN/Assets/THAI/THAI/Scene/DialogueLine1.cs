using UnityEngine;

[System.Serializable]
public class DialogueLine1
{
    public string speaker;   // "NPC" hoặc "Player"
    [TextArea(2, 5)]
    public string sentence;
}
