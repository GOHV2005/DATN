using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("🩸 Health Settings")]
    public float maxHealth = 3f;
    private float currentHealth;

    [Header("⚡ Flash Settings")]
    public float flashDuration = 0.15f;      // Thời gian chớp
    public float flashFrequency = 10f;       // Tần suất chớp (lần/giây)
    public Color flashColor = Color.red;     // Màu chớp (đỏ)

    [Header("💀 On Death")]
    public GameObject dropItemPrefab;
    public int dropItemCount = 1;
    public float dropForce = 5f;
    public float randomAngleRange = 30f;

    private SpriteRenderer[] spriteRenderers;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalColor = spriteRenderers.Length > 0 ? spriteRenderers[0].color : Color.white;
    }

    // 👇 HÀM MỚI: CHỈ NHẬN DAMAGE
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"{name} took {damage} damage. HP: {currentHealth}/{maxHealth}");

        StartCoroutine(FlashEffect());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    System.Collections.IEnumerator FlashEffect()
    {
        Debug.Log($"[Health] Bắt đầu hiệu ứng chớp đỏ trong {flashDuration}s");

        float timer = 0f;
        float flashInterval = 1f / flashFrequency;

        while (timer < flashDuration)
        {
            // Đổi màu đỏ
            foreach (var sr in spriteRenderers)
            {
                sr.color = flashColor;
            }

            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;

            if (timer >= flashDuration) break;

            // Trở lại màu gốc
            foreach (var sr in spriteRenderers)
            {
                sr.color = originalColor;
            }

            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;
        }

        // Đảm bảo trở lại màu gốc sau cùng
        foreach (var sr in spriteRenderers)
        {
            sr.color = originalColor;
        }

        Debug.Log("[Health] Kết thúc hiệu ứng chớp đỏ");
    }

    protected virtual void Die()
    {
        if (dropItemPrefab != null && dropItemCount > 0)
        {
            for (int i = 0; i < dropItemCount; i++)
            {
                SpawnDroppedItem();
            }
        }

        Debug.Log($"{name} died.");
        Destroy(gameObject);
    }

    private void SpawnDroppedItem()
    {
        GameObject itemObj = Instantiate(dropItemPrefab, transform.position, Quaternion.identity);

        Rigidbody2D rb = itemObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 3f;

            float baseAngle = 70f;
            float randomOffset = Random.Range(-randomAngleRange, randomAngleRange);
            float angle = Mathf.Deg2Rad * (baseAngle + randomOffset);

            Vector2 force = new Vector2(Mathf.Cos(angle) * Random.Range(0.5f, 1f), Mathf.Sin(angle)) * dropForce;
            rb.AddForce(force, ForceMode2D.Impulse);
        }
        else
        {
            Debug.LogWarning($"Dropped item {itemObj.name} has no Rigidbody2D!");
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
}