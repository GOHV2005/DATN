using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class NPCDialogue : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;        // UI Text để hiển thị lời thoại
    public GameObject triggerZone;   // vùng trigger zone của minigame
    private bool conversationEnded = false;

    void Start()
    {
        triggerZone.SetActive(false); // ban đầu ẩn trigger zone
    }

    // Bắt đầu hội thoại
    public void StartConversation()
    {
        dialogueText.text = "Xin chào! Tôi có một trò chơi cho bạn.";
        Invoke("ExplainRules", 2f);
    }

    void ExplainRules()
    {
        dialogueText.text =
            "Luật chơi rất đơn giản:\n" +
            "- Có 3 chiếc chuông, chỉ 1 là thật.\n" +
            "- Chuông sẽ được xáo trộn trong 5 giây.\n" +
            "- Nhiệm vụ của bạn là chọn đúng chuông thật.\n" +
            "Nếu đúng, bạn sẽ nhận phần thưởng. Nếu sai, bạn sẽ bị dịch chuyển đi.";

        Invoke("EndConversation", 5f);
    }

    // Kết thúc hội thoại
    public void EndConversation()
    {
        dialogueText.text = "Nếu đã sẵn sàng, hãy nhấn phím E để bắt đầu!";
        conversationEnded = true;
    }

    void Update()
    {
        // Sau khi hội thoại kết thúc, player nhấn E để bật trigger zone
        if (conversationEnded && Input.GetKeyDown(KeyCode.E))
        {
            triggerZone.SetActive(true);
            dialogueText.text = ""; // xóa lời thoại
            conversationEnded = false;
        }
    }
}
