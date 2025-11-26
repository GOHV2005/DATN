// Dialogue.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialogue
{
    public Sprite playerSprite;
    public Sprite npcSprite;
    public List<DialogueLine> lines = new();


    [Space]
    // Sau khi chọn CÓ hoặc KHÔNG
    public Dialogue acceptDialogue;  // Khi chọn CÓ
    public Dialogue refuseDialogue;  // Khi chọn KHÔNG
}