using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public int cardID;               // ID của cặp (0,1,2...)
    public Sprite frontSprite;       // mặt trước
    public Sprite backSprite;        // mặt sau
    private Image image;
    private bool isRevealed = false;
    private Button button;

    private void Awake()
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();
        button.onClick.AddListener(OnCardClick);
    }

    public void Setup(Sprite front, Sprite back, int id)
    {
        frontSprite = front;
        backSprite = back;
        cardID = id;
        HideCard();
    }

    public void OnCardClick()
    {
        if (isRevealed || !button.interactable) return;
        ShowCard();
        GameManager.Instance.OnCardRevealed(this);
    }

    public void ShowCard()
    {
        isRevealed = true;
        image.sprite = frontSprite;
    }

    public void HideCard()
    {
        isRevealed = false;
        image.sprite = backSprite;
    }

    public void Disable()
    {
        button.interactable = false;
    }
}
