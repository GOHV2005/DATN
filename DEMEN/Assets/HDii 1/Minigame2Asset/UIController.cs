using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UIController : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI movesText;
    public Button replayButton;
    public Button quitButton; 

    void Start()
    {
        replayButton.onClick.AddListener(RestartGame);
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void QuitGame()
    {
        Debug.Log("Thoát Game được gọi");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
#else
        Application.Quit(); // 🔹 Thoát hẳn game khi build
#endif
    }

    public void UpdateScore(int newScore)
    {
        scoreText.text = "Score: " + newScore;
    }

    public void UpdateMoves(int movesLeft)
    {
        movesText.text = "Moves: " + movesLeft;
    }
}
