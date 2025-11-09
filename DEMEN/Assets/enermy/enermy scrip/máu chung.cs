using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("🩸 Health Settings")]
    public float maxHealth = 3f;
    private float currentHealth;

    [Header("💀 On Death")]
    public GameObject dropItemPrefab;   // Kéo prefab item vào đây
    public int dropItemCount = 1;       // Số lượng item rơi ra
    public float dropForce = 5f;        // Lực ném item lên
    public float randomAngleRange = 30f; // Độ lệch ngẫu nhiên (độ)

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"{name} took {damage} damage. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        // 🎇 SPAWN ITEM BAY LÊN
        if (dropItemPrefab != null && dropItemCount > 0)
        {
            for (int i = 0; i < dropItemCount; i++)
            {
                SpawnDroppedItem();
            }
        }

        Destroy(gameObject);
    }

    private void SpawnDroppedItem()
    {
        GameObject itemObj = Instantiate(dropItemPrefab, transform.position, Quaternion.identity);

        Rigidbody2D rb = itemObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Bật physics
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 3f; // hoặc giá trị bạn dùng

            // Tạo góc ném ngẫu nhiên: hướng lên + lệch ngang
            float baseAngle = 70f; // Góc cơ bản (hướng lên)
            float randomOffset = Random.Range(-randomAngleRange, randomAngleRange);
            float angle = Mathf.Deg2Rad * (baseAngle + randomOffset);

            // Tính lực
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