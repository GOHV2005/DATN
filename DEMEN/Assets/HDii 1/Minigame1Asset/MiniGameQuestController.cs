using UnityEngine;

public class MiniGameQuestController : MonoBehaviour
{
    [Header("Minigame")]
    public GameObject miniGameCanvas;
    public GameObject player;

    private bool questAccepted = false;

    void Start()
    {
        miniGameCanvas.SetActive(false);
    }

    // Gọi khi player nhấn "Có"
    public void AcceptQuest()
    {
        questAccepted = true;
        StartMiniGame();
    }

    void StartMiniGame()
    {
        miniGameCanvas.SetActive(true);

        // Khóa player
        if (player != null)
            player.SetActive(false);

        Time.timeScale = 0f; // Pause world (minigame vẫn chạy vì dùng UI)
    }

    // Gọi khi thắng hoặc thoát minigame
    public void EndMiniGame()
    {
        miniGameCanvas.SetActive(false);

        if (player != null)
            player.SetActive(true);

        Time.timeScale = 1f;
    }
}
