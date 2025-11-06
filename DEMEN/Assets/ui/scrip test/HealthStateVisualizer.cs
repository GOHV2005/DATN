// HealthStateSpriteVisualizer.cs
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class HealthStateSpriteVisualizer : MonoBehaviour
{
    [Header("References")]
    public PlayerController player; // Kéo Player vào đây (nếu không, tự tìm)

    [Header("Sprites by Health Level")]
    public Sprite fullHealthSprite;    // 100%
    public Sprite highHealthSprite;    // 75%
    public Sprite mediumHealthSprite;  // 50%
    public Sprite lowHealthSprite;     // 25%
    public Sprite criticalHealthSprite; // 0% hoặc <25%

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

        if (ratio >= 0.6f) return fullHealthSprite;
        if (ratio >= 0.4f) return highHealthSprite;
        if (ratio >= 0.2f) return mediumHealthSprite;
        if (ratio >= 0.01f) return lowHealthSprite;
        if (ratio == 0f) return criticalHealthSprite; // ratio < 25% hoặc = 0
        return criticalHealthSprite;
    }
}