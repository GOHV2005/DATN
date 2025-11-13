using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Card Settings")]
    public GameObject cardPrefab;
    public Transform gridParent;
    public Sprite[] cardFronts;   // hình mặt trước (mỗi loại 1 hình)
    public Sprite cardBack;       // hình mặt sau
    public int rows = 4;
    public int cols = 4;

    private List<Card> cards = new List<Card>();
    private Card firstCard;
    private Card secondCard;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GenerateCards();
    }

    void GenerateCards()
    {
        List<int> ids = new List<int>();
        int pairCount = (rows * cols) / 2;

        for (int i = 0; i < pairCount; i++)
        {
            ids.Add(i);
            ids.Add(i);
        }

        // xáo bài
        for (int i = 0; i < ids.Count; i++)
        {
            int rand = Random.Range(0, ids.Count);
            (ids[i], ids[rand]) = (ids[rand], ids[i]);
        }

        // tạo thẻ
        foreach (int id in ids)
        {
            GameObject obj = Instantiate(cardPrefab, gridParent);
            Card card = obj.GetComponent<Card>();
            card.Setup(cardFronts[id], cardBack, id);
            cards.Add(card);
        }
    }

    public void OnCardRevealed(Card card)
    {
        if (firstCard == null)
        {
            firstCard = card;
        }
        else if (secondCard == null)
        {
            secondCard = card;
            StartCoroutine(CheckMatch());
        }
    }

    IEnumerator CheckMatch()
    {
        yield return new WaitForSeconds(0.5f);

        if (firstCard.cardID == secondCard.cardID)
        {
            firstCard.Disable();
            secondCard.Disable();
        }
        else
        {
            firstCard.HideCard();
            secondCard.HideCard();
        }

        firstCard = null;
        secondCard = null;

        CheckWin();
    }

    void CheckWin()
    {
        foreach (Card c in cards)
        {
            if (c.GetComponent<Button>().interactable)
                return;
        }

        Debug.Log("🎉 Bạn đã thắng!");
    }
}
