using UnityEngine;

public enum GemType { Red, Blue, Green, Yellow, Purple, Orange }

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(AudioSource))]   // Bắt buộc phải có AudioSource
public class Gem : MonoBehaviour
{
    public GemType type;
    public int row, col;
    [HideInInspector] public bool isMatched = false;

    [Header("Sound")]
    public AudioClip flipSound;    // Gán file âm thanh trong Inspector

    private SpriteRenderer sr;
    private AudioSource audioSource;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
    }

    public void SetSprite(Sprite s)
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        sr.sprite = s;
    }

    public void SetPosition(int r, int c)
    {
        row = r; col = c;
    }

    // GỌI HÀM NÀY KHI GẠCH ĐƯỢC LẬT / ĐỔI / MATCH
    public void PlayFlipSound()
    {
        if (flipSound != null)
            audioSource.PlayOneShot(flipSound);
    }

    // Ví dụ: gọi thử khi click
    private void OnMouseDown()
    {
        PlayFlipSound();
    }
}
