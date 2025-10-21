using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public enum AttackDirection
{
    Front,
    Back
}

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    // ====== Movement ======
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runMultiplier = 1.6f;
    public float jumpForce = 12f;
    public int maxJumpCount = 2;
    private int jumpCount = 0;

    private bool isGrounded = false;

    // ====== Dash ======
    [Header("Dash")]
    public float dashForce = 18f;
    public float dashTime = 0.18f;
    public float dashCooldown = 0.8f;
    public float dashManaCost = 25f;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private bool isDashing = false;
    private Vector2 dashDirection;

    // ====== Combat / Damage ======
    [Header("Combat")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float damageOnTouch = 20f;

    [Header("Knockback")]
    public float knockbackForce = 12f;
    public float knockbackAirTime = 0.6f;

    // Invincibility frames
    [Header("Invincibility")]
    public float invincibleTime = 0.5f;
    private bool isInvincible = false;
    private float invincibleTimer = 0f;

    // ====== Knockback Lock ======
    [Header("Knockback Settings")]
    public float knockbackDuration = 0.3f;
    private bool isKnockbacked = false;
    private float knockbackTimer = 0f;

    // ====== Mana ======
    [Header("Mana")]
    public float maxMana = 100f;
    public float currentMana = 100f;
    public float manaRegenRate = 12f;

    // ====== UI ======
    [Header("UI - Health")]
    public Image healthFill;
    public Image healthDelay;

    [Header("UI - Mana")]
    public Image manaFill;
    public Image manaDelay;

    [Header("UI Settings - Delay & Speed")]
    public float delayBeforeDrop = 0.5f;
    public float delayDropSpeed = 0.5f;
    public float mainBarHealSpeed = 0.8f;
    public float manaDelayBeforeDrop = 0.5f;
    public float manaDelayDropSpeed = 0.6f;
    public float manaBarHealSpeed = 0.9f;

    // ====== Animation ======
    [Header("Animation")]
    public Animator animator;

    // ====== Invincibility Flash (Multi-Sprite Support) ======
    [Header("Invincibility Flash")]
    public float flashFrequency = 10f;
    private List<SpriteRenderer> spriteRenderers;
    private float flashTimer = 0f;

    // ====== Internal ======
    private Rigidbody2D rb;
    private float horizontalInput;
    private bool facingRight = true;

    private float healthTargetRatio;
    private float healthDelayTimer = 0f;
    private bool healthDelayDropping = false;
    private bool healthMainHealing = false;

    private float manaTargetRatio;
    private float manaDelayTimer = 0f;
    private bool manaDelayDropping = false;
    private bool manaMainHealing = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // LẤY TẤT CẢ SPRITERENDERER CON (KỂ CẢ TRONG CÁC CHILD BỊ DISABLE)
        spriteRenderers = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>(true));

        if (rb == null)
            Debug.LogError("Rigidbody2D not found on Player!");
        if (spriteRenderers.Count == 0)
            Debug.LogWarning("No SpriteRenderers found on Player or its children!");
    }

    void Start()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        currentMana = Mathf.Clamp(currentMana, 0f, maxMana);
        healthTargetRatio = currentHealth / maxHealth;
        manaTargetRatio = currentMana / maxMana;
        UpdateUIImmediate();
    }

    void Update()
    {
        // ====== HIỆU ỨNG NHẤP NHÁY CHO NHIỀU SPRITE ======
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            flashTimer += Time.deltaTime;

            if (flashTimer >= 1f / flashFrequency)
            {
                flashTimer = 0f;
                if (spriteRenderers.Count > 0)
                {
                    bool currentState = spriteRenderers[0].enabled;
                    foreach (var sr in spriteRenderers)
                    {
                        sr.enabled = !currentState;
                    }
                }
            }

            if (invincibleTimer <= 0f)
            {
                isInvincible = false;
                foreach (var sr in spriteRenderers)
                {
                    sr.enabled = true;
                }
            }
        }
        else
        {
            foreach (var sr in spriteRenderers)
            {
                if (!sr.enabled) sr.enabled = true;
            }
        }

        // ====== CÁC LOGIC KHÁC ======
        if (isKnockbacked)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f) isKnockbacked = false;
        }

        if (dashCooldownTimer > 0f) dashCooldownTimer -= Time.deltaTime;
        if (dashTimer > 0f) dashTimer -= Time.deltaTime;
        else if (isDashing) EndDash();

        if (!isDashing && !isKnockbacked)
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            HandleJumpInput();
            HandleDashInput();
        }

        if (!isDashing) RegenerateMana(Time.deltaTime * manaRegenRate);
        UpdateUI();
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        if (!isDashing && !isKnockbacked)
            HandleMovement();
    }

    // ---------------- Animation ----------------
    void UpdateAnimation()
    {
        if (animator == null) return;

        if (isKnockbacked)
        {
            animator.Play("Hurt");
            return;
        }

        if (isDashing)
        {
            animator.Play("Dash");
            return;
        }

        if (!isGrounded)
        {
            if (rb.linearVelocity.y > 0.01f)
            {
                animator.Play("Jump");
            }
            else if (rb.linearVelocity.y < -0.01f)
            {
                animator.Play("Fall");
            }
            return;
        }

        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            animator.Play("Run");
        }
        else
        {
            animator.Play("Idle");
        }
    }

    // ---------------- Movement ----------------
    void HandleMovement()
    {
        float speed = walkSpeed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            speed *= runMultiplier;

        rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);

        if (horizontalInput > 0.01f && !facingRight) Flip();
        else if (horizontalInput < -0.01f && facingRight) Flip();
    }

    void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && jumpCount < maxJumpCount)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpCount++;
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    // ---------------- Dash ----------------
    void HandleDashInput()
    {
        if (Input.GetKeyDown(KeyCode.Q) && dashCooldownTimer <= 0f && currentMana >= dashManaCost)
        {
            Vector2 dir = Mathf.Abs(horizontalInput) > 0.1f
                ? new Vector2(Mathf.Sign(horizontalInput), 0f)
                : (facingRight ? Vector2.right : Vector2.left);
            StartDash(dir.normalized);
        }
    }

    void StartDash(Vector2 dir)
    {
        UseMana(dashManaCost);
        isDashing = true;
        dashDirection = dir;
        dashTimer = dashTime;
        dashCooldownTimer = dashCooldown;

        isInvincible = true;
        invincibleTimer = 0.12f;

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        rb.AddForce(dashDirection * dashForce, ForceMode2D.Impulse);
    }

    void EndDash()
    {
        isDashing = false;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    // ---------------- Ground Collision ----------------
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider != null && collision.collider.CompareTag("Enemy"))
        {
            AttackDirection dir = GetAttackDirection(collision.transform.position);
            TakeDamage(damageOnTouch, dir);
        }

        if (!isKnockbacked && collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            jumpCount = 0;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!isKnockbacked && collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    AttackDirection GetAttackDirection(Vector2 enemyPosition)
    {
        float playerX = transform.position.x;
        float enemyX = enemyPosition.x;
        if (facingRight)
            return (enemyX >= playerX) ? AttackDirection.Front : AttackDirection.Back;
        else
            return (enemyX <= playerX) ? AttackDirection.Front : AttackDirection.Back;
    }

    // ---------------- Knockback ----------------
    float CalculateKnockbackUpForce()
    {
        float gravityMagnitude = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
        return (gravityMagnitude * knockbackAirTime) / 2f;
    }

    public void TakeDamage(float amount, AttackDirection direction)
    {
        if (isInvincible) return;

        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
        healthTargetRatio = (maxHealth > 0f) ? currentHealth / maxHealth : 0f;
        if (healthFill != null) healthFill.fillAmount = healthTargetRatio;

        healthDelayTimer = delayBeforeDrop;
        healthDelayDropping = true;
        healthMainHealing = false;

        float forceMultiplier = (direction == AttackDirection.Back) ? 1.5f : 1f;
        float knockbackX = (direction == AttackDirection.Front)
            ? (facingRight ? -knockbackForce * forceMultiplier : knockbackForce * forceMultiplier)
            : (facingRight ? knockbackForce * forceMultiplier : -knockbackForce * forceMultiplier);

        float knockbackY = CalculateKnockbackUpForce();

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(knockbackX, knockbackY), ForceMode2D.Impulse);

        isKnockbacked = true;
        knockbackTimer = knockbackDuration;

        isInvincible = true;
        invincibleTimer = invincibleTime;

        Debug.Log($"Bị đánh từ {(direction == AttackDirection.Front ? "TRƯỚC MẶT" : "SAU LƯNG")}!");

        if (currentHealth <= 0f) Die();
    }

    // ---------------- UI & Mana ----------------
    public void Heal(float amount)
    {
        if (amount <= 0f) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
        healthTargetRatio = currentHealth / maxHealth;

        if (healthDelay != null) healthDelay.fillAmount = healthTargetRatio;

        healthMainHealing = true;
        healthDelayDropping = false;
        healthDelayTimer = 0f;
    }

    void UseMana(float amount)
    {
        if (amount <= 0f) return;
        currentMana = Mathf.Clamp(currentMana - amount, 0f, maxMana);
        manaTargetRatio = currentMana / maxMana;

        if (manaFill != null) manaFill.fillAmount = manaTargetRatio;

        manaDelayTimer = manaDelayBeforeDrop;
        manaDelayDropping = true;
        manaMainHealing = false;
    }

    public void RestoreMana(float amount)
    {
        if (amount <= 0f) return;
        currentMana = Mathf.Clamp(currentMana + amount, 0f, maxMana);
        manaTargetRatio = currentMana / maxMana;

        if (manaDelay != null) manaDelay.fillAmount = manaTargetRatio;

        manaMainHealing = true;
        manaDelayDropping = false;
        manaDelayTimer = 0f;
    }

    void RegenerateMana(float amount)
    {
        if (amount <= 0f) return;
        float prev = currentMana;
        currentMana = Mathf.Clamp(currentMana + amount, 0f, maxMana);
        if (currentMana != prev)
        {
            manaTargetRatio = currentMana / maxMana;
            if (manaDelay != null) manaDelay.fillAmount = manaTargetRatio;
            manaMainHealing = true;
            manaDelayDropping = false;
            manaDelayTimer = 0f;
        }
    }

    void UpdateUI()
    {
        healthTargetRatio = currentHealth / maxHealth;
        manaTargetRatio = currentMana / maxMana;

        if (healthFill != null)
        {
            if (healthMainHealing)
            {
                healthFill.fillAmount = Mathf.MoveTowards(healthFill.fillAmount, healthTargetRatio, mainBarHealSpeed * Time.deltaTime);
                if (Mathf.Approximately(healthFill.fillAmount, healthTargetRatio)) healthMainHealing = false;
            }
            else
            {
                healthFill.fillAmount = healthTargetRatio;
            }
        }

        if (healthDelay != null && healthFill != null)
        {
            if (healthDelay.fillAmount > healthTargetRatio && healthDelayDropping)
            {
                if (healthDelayTimer > 0f)
                    healthDelayTimer -= Time.deltaTime;
                else
                    healthDelay.fillAmount = Mathf.MoveTowards(healthDelay.fillAmount, healthTargetRatio, delayDropSpeed * Time.deltaTime);
            }
        }

        if (manaFill != null)
        {
            if (manaMainHealing)
            {
                manaFill.fillAmount = Mathf.MoveTowards(manaFill.fillAmount, manaTargetRatio, manaBarHealSpeed * Time.deltaTime);
                if (Mathf.Approximately(manaFill.fillAmount, manaTargetRatio)) manaMainHealing = false;
            }
            else
            {
                manaFill.fillAmount = manaTargetRatio;
            }
        }

        if (manaDelay != null && manaFill != null)
        {
            if (manaDelay.fillAmount > manaTargetRatio && manaDelayDropping)
            {
                if (manaDelayTimer > 0f)
                    manaDelayTimer -= Time.deltaTime;
                else
                    manaDelay.fillAmount = Mathf.MoveTowards(manaDelay.fillAmount, manaTargetRatio, manaDelayDropSpeed * Time.deltaTime);
            }
        }
    }

    void UpdateUIImmediate()
    {
        float healthRatio = currentHealth / maxHealth;
        float manaRatio = currentMana / maxMana;

        if (healthFill != null) healthFill.fillAmount = healthRatio;
        if (manaFill != null) manaFill.fillAmount = manaRatio;
        if (healthDelay != null) healthDelay.fillAmount = healthRatio;
        if (manaDelay != null) manaDelay.fillAmount = manaRatio;

        healthDelayDropping = false;
        healthMainHealing = false;
        healthDelayTimer = 0f;
        manaDelayDropping = false;
        manaMainHealing = false;
        manaDelayTimer = 0f;
    }

    void Die()
    {
        isInvincible = false;
        foreach (var sr in spriteRenderers)
        {
            sr.enabled = true; // hoặc false nếu muốn ẩn toàn bộ
        }

        if (animator != null)
            animator.enabled = false;

        Debug.Log("Player died");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other != null && other.CompareTag("Enemy"))
        {
            AttackDirection dir = GetAttackDirection(other.transform.position);
            TakeDamage(damageOnTouch, dir);
        }
    }
}