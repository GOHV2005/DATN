using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Card Setup")]
    public GameObject cardPrefab;
    public Transform cardParent;
    public Sprite[] frontSprites; // sprite mặt trước
    public Sprite backSprite;     // sprite mặt sau
    public int moves = 20;        // số lượt

    [Header("UI")]
    public TextMeshProUGUI movesText;
    public TextMeshProUGUI pairsText;
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Sound")]
    public AudioClip matchSound;
    public AudioClip loseSound;
    private AudioSource audioSource;

    [Header("Minigame Controller")]
    public MinigameController minigameController;

    private List<Card> cards = new List<Card>();
    private Card firstRevealed;
    private Card secondRevealed;
    private int pairsFound = 0;

    private void Awake()
    {
        Instance = this;
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        SetupGame();
    }

    void SetupGame()
    {
        // Reset UI
        winPanel.SetActive(false);
        losePanel.SetActive(false);

        pairsFound = 0;
        pairsText.text = "Số cặp: " + pairsFound;

        movesText.text = "Moves: " + moves;

        // Xóa thẻ cũ nếu có
        foreach (Card c in cards)
        {
            Destroy(c.gameObject);
        }
        cards.Clear();

        // Tạo danh sách cặp ID
        List<int> ids = new List<int>();
        for (int i = 0; i < frontSprites.Length; i++)
        {
            ids.Add(i);
            ids.Add(i);
        }

        Shuffle(ids);

        // Spawn thẻ
        for (int i = 0; i < ids.Count; i++)
        {
            GameObject obj = Instantiate(cardPrefab, cardParent);
            Card card = obj.GetComponent<Card>();
            card.cardID = ids[i];
            card.frontSprite = frontSprites[ids[i]];
            card.backSprite = backSprite;
            cards.Add(card);
        }
    }

    public void CardRevealed(Card card)
    {
        if (firstRevealed == null)
        {
            firstRevealed = card;
        }
        else if (secondRevealed == null)
        {
            secondRevealed = card;
            moves--;
            movesText.text = "Moves: " + moves;
            StartCoroutine(CheckMatch());
        }
    }

    System.Collections.IEnumerator CheckMatch()
    {
        yield return new WaitForSeconds(0.5f);

        if (firstRevealed.cardID == secondRevealed.cardID)
        {
            pairsFound++;
            audioSource.PlayOneShot(matchSound);
            pairsText.text = "Số cặp: " + pairsFound;
        }
        else
        {
            firstRevealed.Hide();
            secondRevealed.Hide();
        }

        firstRevealed = null;
        secondRevealed = null;

        // ===== KIỂM TRA THẮNG / THUA =====
        if (pairsFound >= frontSprites.Length)
        {
            winPanel.SetActive(true); // 🔥 CHỈ HIỆN PANEL – KHÔNG THOÁT
        }
        else if (moves <= 0)
        {
            losePanel.SetActive(true);
            audioSource.PlayOneShot(loseSound);
        }
    }

    // ===============================
    // NÚT "THOÁT" TRONG WIN PANEL
    // ===============================
    public void OnExitAfterWin()
    {
        winPanel.SetActive(false);

        if (minigameController != null)
        {
            minigameController.EndMinigame(); // 🔥 ẨN CANVAS MINIGAME + HIỆN SWITCH
        }
    }

    public void PlayAgain()
    {
        moves = 20;
        SetupGame();
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
