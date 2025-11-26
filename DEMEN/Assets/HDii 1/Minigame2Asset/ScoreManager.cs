using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public int Score { get; private set; }
    public TextMeshProUGUI scoreText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    public int GetScore()
    {
        return Score;
    }


    public void AddScore(int matchCount)
    {
        int points = 0;

        if (matchCount == 3) points = 100;
        else if (matchCount == 4) points = 200;
        else if (matchCount >= 5) points = 500;

        Score += points;

        if (scoreText != null)
            scoreText.text = "Score: " + Score;
    }
}
