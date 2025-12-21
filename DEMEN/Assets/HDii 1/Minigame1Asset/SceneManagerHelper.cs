using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerHelper : MonoBehaviour
{
    public static SceneManagerHelper Instance;

    private string savedSceneName;
    private Vector3 savedPlayerPosition;

    public bool minigameWonStatus = false; // Lưu trạng thái thắng

    private void Awake()
    {
        Instance = this; // Không dùng DontDestroyOnLoad
    }

    // Chuyển sang minigame
    public void GoToMinigame(string minigameName, Transform player)
    {
        savedSceneName = SceneManager.GetActiveScene().name;
        savedPlayerPosition = player.position;

        SceneManager.LoadScene(minigameName);
    }

    public void GoToMinigame(string minigameName)
    {
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player not found!");
            return;
        }
        GoToMinigame(minigameName, player);
    }

    // Quay về scene chính, player thắng hay không
    public void ReturnToPreviousScene(bool playerWon)
    {
        minigameWonStatus = playerWon;

        SceneManager.sceneLoaded += OnMainSceneLoaded;
        SceneManager.LoadScene(savedSceneName);
    }

    public void ReturnToPreviousScene()
    {
        ReturnToPreviousScene(false);
    }

    private void OnMainSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnMainSceneLoaded;

        // Khôi phục player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerObj.transform.position = savedPlayerPosition;

        // Nếu thắng minigame → gọi gate controller
        if (minigameWonStatus)
        {
            MinigameGateController gate = FindObjectOfType<MinigameGateController>();
            if (gate != null)
                gate.OnMinigameWon();
        }
    }
}
