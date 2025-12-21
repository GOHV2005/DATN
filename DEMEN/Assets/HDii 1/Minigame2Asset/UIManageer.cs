using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManageer : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI movesText;
    public GameObject winPanel;
    public GameObject losePanel;

    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    public void UpdateMoves(int moves)
    {
        if (movesText != null)
            movesText.text = "Moves: " + moves;
    }

    public void ShowWin()
    {
        if (winPanel != null)
            winPanel.SetActive(true);
    }

    public void ShowLose()
    {
        if (losePanel != null)
            losePanel.SetActive(true);
    }
}
