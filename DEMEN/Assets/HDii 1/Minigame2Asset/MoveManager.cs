using TMPro;
using UnityEngine;

public class MoveManager : MonoBehaviour
{
    public static MoveManager Instance;

    public int moves = 20;
    public TextMeshProUGUI movesText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateUI();
    }

    public void UseMove()
    {
        moves--;

        UpdateUI();

        // GỌI KIỂM TRA ENDGAME
        GridManager.Instance.CheckEndGame();
    }

    // Hàm GridManager gọi vào để lấy số lượt còn lại
    public int GetMoves()
    {
        return moves;
    }

    private void UpdateUI()
    {
        if (movesText != null)
            movesText.text = "Moves: " + moves;
    }
}
