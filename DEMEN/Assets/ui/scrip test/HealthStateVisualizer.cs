// GrasshopperHeadUI.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class GrasshopperHeadUI : MonoBehaviour
{
    [Header("Sprites by Health (5 sprites)")]
    public Sprite fullHealthSprite;       // 5/5 máu
    public Sprite highHealthSprite;       // 4/5 máu
    public Sprite mediumHealthSprite;     // 3/5 máu
    public Sprite lowHealthSprite;        // 2/5 máu
    public Sprite criticalHealthSprite;   // 1/5 máu

    [Header("Flash & Shake Settings")]
    public Material flashMaterial;        // 👈 KÉO MATERIAL VÀO ĐÂY
    public float flashDuration = 0.1f;    // Thời gian chớp

    public bool enableShake = true;       // Cho phép rung
    public float shakeAmount = 10f;       // Mức rung (pixel)
    public float shakeDuration = 0.15f;   // Thời gian rung

    private Image image;
    private Sprite currentSprite;
    private Material originalMaterial;
    private Vector3 originalLocalPosition;
    private PlayerController player;

    void Awake()
    {
        image = GetComponent<Image>();
        originalMaterial = image.material;
        originalLocalPosition = transform.localPosition;

        player = Object.FindAnyObjectByType<PlayerController>();
        if (player == null)
            Debug.LogError("PlayerController not found! Please assign manually.");
    }

    void Start()
    {
        // 👇 GỌI HÀM CẬP NHẬT SPRITE BAN ĐẦU
        Sprite newSprite = GetHealthSprite();
        currentSprite = newSprite;
        image.sprite = newSprite;
    }

    void OnEnable()
    {
        if (player != null)
        {
            player.onTakeDamage += OnPlayerTakeDamage;
        }
    }

    void OnDisable()
    {
        if (player != null)
        {
            player.onTakeDamage -= OnPlayerTakeDamage;
        }
    }

    void Update()
    {
        if (player == null) return;

        // Cập nhật sprite nếu máu thay đổi
        Sprite newSprite = GetHealthSprite();
        if (newSprite != currentSprite)
        {
            currentSprite = newSprite;
            image.sprite = newSprite;
        }
    }

    void OnPlayerTakeDamage()
    {
        // 👇 CHỈ CHỚP TRẮNG + RUNG KHI PLAYER BỊ ĐÁNH
        if (flashMaterial != null)
        {
            StartCoroutine(FlashAndShake());
        }
    }

    Sprite GetHealthSprite()
    {
        if (player.isDead) return criticalHealthSprite; // Nếu chết → sprite cuối

        float ratio = player.CurrentHealth / player.maxHealth;
        int healthLevel = Mathf.CeilToInt(ratio * 5); // 5 cấp độ
        healthLevel = Mathf.Clamp(healthLevel, 1, 5);

        switch (healthLevel)
        {
            case 5: return fullHealthSprite;
            case 4: return fullHealthSprite;      // 5/5 máu
            case 3: return highHealthSprite;      // 4/5 máu
            case 2: return mediumHealthSprite;    // 3/5 máu
            case 1: return lowHealthSprite;       // 2/5 máu
            case 0:
            default:
                return criticalHealthSprite;      // 1/5 máu
        }
    }

    IEnumerator FlashAndShake()
    {
        // 👇 CHỚP TRẮNG
        image.material = flashMaterial;
        yield return new WaitForSeconds(flashDuration);
        image.material = originalMaterial;

        // 👇 RUNG LẮC TRÁI-PHẢI
        if (enableShake)
        {
            Vector3 originalPos = transform.localPosition;
            float elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                float x = Mathf.Sin(elapsed * 30f) * shakeAmount; // Lắc trái-phải
                transform.localPosition = originalPos + new Vector3(x, 0f, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = originalPos;
        }
    }
}