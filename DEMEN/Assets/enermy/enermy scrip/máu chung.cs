using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("🩸 Health Settings")]
    public float maxHealth = 3f;
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    // Gọi khi nhận sát thương
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

    // Gọi khi chết
    protected virtual void Die()
    {
        // 🎇 Có thể thêm: particle, sound, drop item...
        Destroy(gameObject);
    }

    // Optional: hồi máu
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    // Optional: lấy máu hiện tại (dành cho hệ thống khác)
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;


    /*// Ví dụ trong ve sầu hoặc player:
    void OnCollisionEnter2D(Collision2D col)
    {
    Health health = col.gameObject.GetComponent<Health>();
    if (health != null)
    {
        health.TakeDamage(1f); // Gây 1 sát thương
    }
    }*/

    //gây satsthuowng lên player
    /*void OnAttack()
    {
    // Tìm player (đã có biến `player` trong script)
    if (player != null)
    {
    Health playerHealth = player.GetComponent<Health>();
    if (playerHealth != null)
    {
        playerHealth.TakeDamage(1f); // Gây 1 sát thương
    }
    }*/
}
