// DialogueLine.cs
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    [Header("Speaker")]
    public bool isPlayer;

    [Header("Text")]
    [TextArea(2, 4)]
    public string text;

    [Header("Camera")]
    public bool enableCameraFocus;          // ✅ Có lia camera không
    public Transform cameraFocusTarget;     // ✅ Lia tới đâu
}
