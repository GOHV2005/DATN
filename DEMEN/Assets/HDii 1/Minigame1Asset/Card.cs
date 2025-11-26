using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Card : MonoBehaviour
{
    public int cardID; // ID cặp
    public Sprite frontSprite;
    public Sprite backSprite;
    private Image image;
    private bool isRevealed = false;
    private Button button;

    [Header("Sound")]
    public AudioClip flipSound;
    private AudioSource audioSource;

    private void Awake()
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();
        audioSource = gameObject.AddComponent<AudioSource>();
        image.sprite = backSprite;
        button.onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        if (!isRevealed)
        {
            StartCoroutine(FlipCard());
        }
    }

    IEnumerator FlipCard()
    {
        audioSource.PlayOneShot(flipSound);
        // Scale về 0 trên trục X
        float duration = 0.15f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.localScale = new Vector3(Mathf.Lerp(1, 0, elapsed / duration), 1, 1);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = new Vector3(0, 1, 1);

        // Đổi sprite
        image.sprite = isRevealed ? backSprite : frontSprite;
        isRevealed = !isRevealed;

        // Scale lại
        elapsed = 0f;
        while (elapsed < duration)
        {
            transform.localScale = new Vector3(Mathf.Lerp(0, 1, elapsed / duration), 1, 1);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = new Vector3(1, 1, 1);

        // Báo GameManager
        GameManager.Instance.CardRevealed(this);
    }

    public void Hide()
    {
        isRevealed = false;
        image.sprite = backSprite;
    }

    public bool IsRevealed()
    {
        return isRevealed;
    }
}
