// HealthStateSpriteVisualizer.cs
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class HealthStateSpriteVisualizer : MonoBehaviour
{
    [Header("References")]
    public PlayerController player; // Kéo Player vào đây (nếu không, tự tìm)

    [Header("Sprites by Health Level")]
    public Sprite fullHealthSprite;       // 100%
    public Sprite highHealthSprite;       // 75%
    public Sprite mediumHealthSprite;     // 50%
    public Sprite lowHealthSprite;        // 25%
    public Sprite criticalHealthSprite;   // 0%

    private Image image;
    private Sprite currentSprite;

    void Awake()
    {
        image = GetComponent<Image>();
        if (player == null)
        {
            player = Object.FindAnyObjectByType<PlayerController>();
            if (player == null)
                Debug.LogError("PlayerController not found! Please assign manually.");
        }
    }

    void Update()
    {
        if (player == null || player.isDead) return;

        Sprite newSprite = GetHealthSprite();
        if (newSprite != currentSprite)
        {
            currentSprite = newSprite;
            image.sprite = newSprite;
        }
    }

    Sprite GetHealthSprite()
    {
        float ratio = player.CurrentHealth / player.maxHealth;

        if (Mathf.Approximately(ratio, 1f) || ratio > 0.75f) return fullHealthSprite;      // 100% – 76%
        if (ratio > 0.50f) return highHealthSprite;                                       // 75% – 51%
        if (ratio > 0.25f) return mediumHealthSprite;                                     // 50% – 26%
        if (ratio > 0f) return lowHealthSprite;                                           // 25% – 0.1%
        return criticalHealthSprite;                                                      // 0%
    }
}