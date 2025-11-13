using UnityEngine;
using UnityEngine.SceneManagement;

public class GameeManager : MonoBehaviour
{
    public static GameeManager Instance;

    public UIManageer ui;
    public GridManager grid;

    private int score = 0;
    private int moves = 25;
    private int targetScore = 15000;
    private bool gameEnded = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (ui != null)
        {
            ui.UpdateScore(score);
            ui.UpdateMoves(moves);
        }
        else
        {
            Debug.LogWarning("UIManager chưa được gán trong GameManager!");
        }
    }

    public void AddScore(int amount)
    {
        if (gameEnded) return;

        score += amount;
        ui.UpdateScore(score);

        if (score >= targetScore)
        {
            gameEnded = true;
            ui.ShowWin();
        }
    }

    public void UseMove()
    {
        if (gameEnded) return;

        moves--;
        ui.UpdateMoves(moves);

        if (moves <= 0 && score < targetScore)
        {
            gameEnded = true;
            ui.ShowLose();
        }
    }

    public void Replay()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Skip()
    {
        Debug.Log("Bỏ qua mini game");
    }
}
