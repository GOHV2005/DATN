using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    public System.Action onDeath;
    [Header("🩸 Health Settings")]
    public float maxHealth = 3f;
    public float currentHealth;

    [Header("💥 Knockback Settings")]
    public float knockbackForce = 30f;
    public float knockbackDuration = 0.15f;

    [Header("⚡ Flash Material Settings")]
    public Material flashMaterial;          // 👈 Kéo vào đây material FlashDamage
    public float flashDuration = 0.1f;      // Thời gian sáng trắng

    [Header("💀 On Death")]
    public GameObject dropItemPrefab;
    public int dropItemCount = 1;
    public float dropForce = 5f;
    public float randomAngleRange = 30f;

    [Header("💉 Blood Effect")]
    public GameObject bloodPrefab;
    public int bloodCount = 5; // số hạt máu
    public float bloodForce = 1f; // lực tóe

    private SpriteRenderer[] spriteRenderers;
    private Material[] originalMaterials;
    private Rigidbody2D rb;
    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalMaterials = new Material[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
            originalMaterials[i] = spriteRenderers[i].sharedMaterial;
    }
    public void TakeDamage(float damage)
    {
        Vector2 knockbackDir = ((Vector2)transform.position - GetPlayerPosition()).normalized;
        TakeDamage(damage, knockbackDir);
    }

    public void TakeDamage(float damage, Transform attacker)
    {
        Vector2 knockbackDir = ((Vector2)transform.position - (Vector2)attacker.position).normalized;
        TakeDamage(damage, knockbackDir);
    }

    public void TakeDamage(float damage, Vector2 knockbackDirection)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (rb != null)
            rb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);

        StartCoroutine(FlashMaterialEffect());

        // SỬ DỤNG knockbackDirection thay vì attacker
        SpawnBlood(knockbackDirection);

        // 👇 GỌI HÀM OnTakeDamage CHO ENEMY (DÙNG SendMessage - AN TOÀN)
        SendMessage("OnTakeDamage", SendMessageOptions.DontRequireReceiver);

        if (currentHealth <= 0)
            Die();
    }

    private void SpawnBlood(Vector2 knockbackDir)
    {
        if (bloodPrefab == null) return;

        for (int i = 0; i < bloodCount; i++)
        {
            GameObject blood = Instantiate(bloodPrefab, transform.position, Quaternion.identity);

            Blood bloodScript = blood.GetComponent<Blood>();
            if (bloodScript != null)
            {
                // Kết hợp với hướng knockback và chút ngẫu nhiên
                Vector2 randomDir = (knockbackDir + Random.insideUnitCircle * 0.5f).normalized;
                bloodScript.SetDirection(randomDir);
            }

            // Optional: nếu prefab có Rigidbody2D
            Rigidbody2D rb = blood.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(Random.insideUnitCircle * bloodForce, ForceMode2D.Impulse);
            }
        }
    }

    IEnumerator FlashMaterialEffect()
    {
        if (flashMaterial == null) yield break;

        // 🔥 Đổi tất cả SpriteRenderer sang flashMaterial
        foreach (var sr in spriteRenderers)
            sr.material = flashMaterial;

        yield return new WaitForSeconds(flashDuration);

        // 🔄 Trả lại material gốc
        for (int i = 0; i < spriteRenderers.Length; i++)
            spriteRenderers[i].material = originalMaterials[i];
    }

    Vector2 GetPlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) return player.transform.position;
        return (Vector2)transform.position;
    }

    protected virtual void Die()
    {
        onDeath?.Invoke();
        if (dropItemPrefab != null && dropItemCount > 0)
        {
            for (int i = 0; i < dropItemCount; i++)
                SpawnDroppedItem();
        }
        EnemyDeathHandler.OnEnemyDied?.Invoke();
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
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
}