using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("🩸 Health Settings")]
    public float maxHealth = 3f;
    private float currentHealth;

    [Header("💥 Knockback Settings")]
    public float knockbackForce = 30f;
    public float knockbackDuration = 0.15f;

    [Header("⚡ Flash Settings")]
    public float flashDuration = 0.15f;
    public float flashFrequency = 10f;
    public Color flashColor = Color.red;

    [Header("💀 On Death")]
    public GameObject dropItemPrefab;
    public int dropItemCount = 1;
    public float dropForce = 5f;
    public float randomAngleRange = 30f;

    private SpriteRenderer[] spriteRenderers;
    private Color originalColor;
    private Rigidbody2D rb;

    void Start()
    {
        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody2D>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalColor = spriteRenderers.Length > 0 ? spriteRenderers[0].color : Color.white;
    }

    // 👇 HÀM GỐC: CHỈ NHẬN DAMAGE (dùng chung)
    public void TakeDamage(float damage)
    {
        Vector2 knockbackDir = ((Vector2)transform.position - GetPlayerPosition()).normalized;
        TakeDamage(damage, knockbackDir);
    }

    // 👇 HÀM MỚI: NHẬN DAMAGE + TRANSFORM (từ kẻ tấn công)
    public void TakeDamage(float damage, Transform attacker)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"{name} took {damage} damage from {attacker.name}. HP: {currentHealth}/{maxHealth}");

        if (rb != null)
        {
            Vector2 knockbackDir = ((Vector2)transform.position - (Vector2)attacker.position).normalized;
            rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
        }

        StartCoroutine(FlashEffect());

        if (currentHealth <= 0)
            Die();
    }

    // 👇 HÀM PHỤ: NHẬN DAMAGE + HƯỚNG ĐẨY
    public void TakeDamage(float damage, Vector2 knockbackDirection)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"{name} took {damage} damage. HP: {currentHealth}/{maxHealth}");

        if (rb != null)
        {
            rb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);
        }

        StartCoroutine(FlashEffect());

        if (currentHealth <= 0)
            Die();
    }

    Vector2 GetPlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) return player.transform.position;

        if (PlayerController.Instance != null) return PlayerController.Instance.transform.position;

        return (Vector2)transform.position;
    }

    System.Collections.IEnumerator FlashEffect()
    {
        Debug.Log($"[Health] Bắt đầu hiệu ứng chớp đỏ trong {flashDuration}s");

        float timer = 0f;
        float flashInterval = 1f / flashFrequency;

        while (timer < flashDuration)
        {
            foreach (var sr in spriteRenderers)
            {
                sr.color = flashColor;
            }

            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;

            if (timer >= flashDuration) break;

            foreach (var sr in spriteRenderers)
            {
                sr.color = originalColor;
            }

            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;
        }

        foreach (var sr in spriteRenderers)
        {
            sr.color = originalColor;
        }

        // 👇 GỌI HÀM TỪ ENEMY ĐỂ PHẢN ĐÒN
        EnemyAnt antAI = GetComponent<EnemyAnt>();
        if (antAI != null)
        {
            antAI.OnTakeDamage();
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