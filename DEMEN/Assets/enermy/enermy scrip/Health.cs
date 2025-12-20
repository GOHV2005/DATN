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

    [Header("⚡ Flash Material Settings")]
    public Material flashMaterial;
    public float flashDuration = 0.1f;

    [Header("💀 On Death")]
    public GameObject dropItemPrefab;
    public int dropItemCount = 1;
    public float dropForce = 5f;
    public float randomAngleRange = 30f;

    [Header("💉 Blood Effect")]
    public GameObject bloodPrefab;
    public int bloodCount = 5;
    public float bloodForce = 1f;

    [Header("💀 Death Options")]
    public bool useDissolve = true;            // Chọn dissolve hay particle
    public Dissovle _dissolve;                 // Gán Dissolve component nếu dùng dissolve
    public ParticleSystem deathParticle;
    public AudioClip deathSound;
    public AudioSource audioSource;

    private SpriteRenderer spriteRenderer;
    private Material originalMaterial;
    private Rigidbody2D rb;
    private Collider2D col;
    private bool isDead = false;
    private int lastAttackId = -1;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalMaterial = spriteRenderer.material;
    }

    #region Damage
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
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (rb != null)
            rb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);

        StartCoroutine(FlashMaterialEffect());
        SpawnBlood(knockbackDirection);

        SendMessage("OnTakeDamage", SendMessageOptions.DontRequireReceiver);

        if (currentHealth <= 0)
            Die();
    }

    public bool CanTakeDamage(int attackId)
    {
        if (lastAttackId == attackId) return false;
        lastAttackId = attackId;
        return true;
    }
    #endregion

    #region Visual & Blood
    private void SpawnBlood(Vector2 knockbackDir)
    {
        if (bloodPrefab == null) return;

        for (int i = 0; i < bloodCount; i++)
        {
            GameObject blood = Instantiate(bloodPrefab, transform.position, Quaternion.identity);
            Blood bloodScript = blood.GetComponent<Blood>();
            if (bloodScript != null)
            {
                Vector2 randomDir = (knockbackDir + Random.insideUnitCircle * 0.5f).normalized;
                bloodScript.SetDirection(randomDir);
            }

            Rigidbody2D rbBlood = blood.GetComponent<Rigidbody2D>();
            if (rbBlood != null)
            {
                rbBlood.AddForce(Random.insideUnitCircle * bloodForce, ForceMode2D.Impulse);
            }
        }
    }

    IEnumerator FlashMaterialEffect()
    {
        if (flashMaterial == null || spriteRenderer == null) yield break;

        spriteRenderer.material = flashMaterial;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.material = originalMaterial;
    }
    #endregion

    Vector2 GetPlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) return player.transform.position;
        return (Vector2)transform.position;
    }

    #region Death
    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        // Stop motion & lock physics
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (col != null)
            col.enabled = false;

        // Lock any skill / AI behavior
        SendMessage("OnDeath", SendMessageOptions.DontRequireReceiver);

        onDeath?.Invoke();

        // Death visuals
        if (useDissolve && _dissolve != null)
        {
            StartCoroutine(VanishThenDestroy());
        }
        else
        {
            if (deathParticle != null)
                Instantiate(deathParticle, transform.position, transform.rotation).Play();

            if (audioSource != null && deathSound != null)
                audioSource.PlayOneShot(deathSound);

            if (spriteRenderer != null) spriteRenderer.enabled = false;

            Destroy(gameObject, 1f);
        }

        if (dropItemPrefab != null && dropItemCount > 0)
        {
            for (int i = 0; i < dropItemCount; i++)
                SpawnDroppedItem();
        }

        EnemyDeathHandler.OnEnemyDied?.Invoke();
    }

    private IEnumerator VanishThenDestroy()
    {
        if (_dissolve != null)
            yield return StartCoroutine(_dissolve.Vanish(true, false));

        Destroy(gameObject, 0.1f);
    }

    private void SpawnDroppedItem()
    {
        GameObject itemObj = Instantiate(dropItemPrefab, transform.position, Quaternion.identity);
        Rigidbody2D rbItem = itemObj.GetComponent<Rigidbody2D>();
        if (rbItem != null)
        {
            rbItem.bodyType = RigidbodyType2D.Dynamic;
            rbItem.gravityScale = 3f;

            float baseAngle = 70f;
            float randomOffset = Random.Range(-randomAngleRange, randomAngleRange);
            float angle = Mathf.Deg2Rad * (baseAngle + randomOffset);

            Vector2 force = new Vector2(Mathf.Cos(angle) * Random.Range(0.5f, 1f), Mathf.Sin(angle)) * dropForce;
            rbItem.AddForce(force, ForceMode2D.Impulse);
        }
    }
    #endregion

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
}
