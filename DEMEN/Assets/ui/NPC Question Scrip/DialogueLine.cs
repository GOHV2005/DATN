// DialogueLine.cs
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    [Header("=== Người nói ===")]
    public bool isPlayer;

    [Header("=== Văn bản ===")]
    [TextArea(2, 4)]
    public string text;

    [Header("=== Loại đoạn hội thoại ===")]
    public bool isQuestRequest;         // Dòng yêu cầu nhiệm vụ → hiện nút CÓ/KHÔNG
    public bool isAcceptLine;           // Dòng sau khi chọn CÓ
    public bool isRefuseLine;           // Dòng sau khi chọn KHÔNG

    public bool isMissingItem;          // Thiếu item
    public bool isQuestComplete;        // Đã đủ item → hoàn thành
    public bool isDoneIfPlayerComeBack; // Đã hoàn thành, player quay lại
}