using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerHelper : MonoBehaviour
{
    public static SceneManagerHelper Instance;

    private string previousScene; // lưu Scene trước khi vào minigame

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // giữ object qua các scene
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GoToMinigame(string minigameSceneName)
    {
        previousScene = SceneManager.GetActiveScene().name; // lưu scene hiện tại
        SceneManager.LoadScene(minigameSceneName);
    }

    public void ReturnToPreviousScene()
    {
        if (!string.IsNullOrEmpty(previousScene))
        {
            SceneManager.LoadScene(previousScene);
        }
        else
        {
            Debug.LogWarning("Previous scene not set!");
        }
    }
}
