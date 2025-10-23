using UnityEngine;
using UnityEngine.UI;

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

    // Ground check
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.12f;
    public LayerMask groundLayer;
    private bool isGrounded;
    public float groundRaycastDistance = 0.15f;

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
    public float knockbackHorizontal = 10f; // lực đẩy ngang
    public float knockbackVertical = 8f;    // lực hất lên

    // Invincibility frames
    [Header("Invincibility")]
    public float invincibleTime = 0.5f;
    private bool isInvincible = false;
    private float invincibleTimer = 0f;

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
        if (rb == null)
            Debug.LogError("Rigidbody2D not found on Player!");
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
        CheckGround();

        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer <= 0f) isInvincible = false;
        }

        if (dashCooldownTimer > 0f) dashCooldownTimer -= Time.deltaTime;
        if (dashTimer > 0f) dashTimer -= Time.deltaTime;
        else if (isDashing) EndDash();

        if (!isDashing)
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            HandleJumpInput();
            HandleDashInput();
        }

        if (!isDashing) RegenerateMana(Time.deltaTime * manaRegenRate);
        UpdateUI();
    }

    void FixedUpdate()
    {
        if (!isDashing)
            HandleMovement();
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
        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumpCount)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpCount++;
        }
    }

    void CheckGround()
    {
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        else
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundRaycastDistance, groundLayer);
            isGrounded = hit.collider != null;
        }

        if (isGrounded) jumpCount = 0;
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 s = transform.localScale;
        s.x *= -1f;
        transform.localScale = s;
    }

    // ---------------- Dash ----------------
    void HandleDashInput()
    {
        if (Input.GetKeyDown(KeyCode.Q) && dashCooldownTimer <= 0f && currentMana >= dashManaCost)
        {
            Vector2 dir = Mathf.Abs(horizontalInput) > 0.1f ? new Vector2(Mathf.Sign(horizontalInput), 0f) : (facingRight ? Vector2.right : Vector2.left);
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

    // ---------------- Health & Knockback ----------------
    public void TakeDamage(float amount)
    {
        if (isInvincible) return;

        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
        healthTargetRatio = (maxHealth > 0f) ? currentHealth / maxHealth : 0f;
        if (healthFill != null) healthFill.fillAmount = healthTargetRatio;

        healthDelayTimer = delayBeforeDrop;
        healthDelayDropping = true;
        healthMainHealing = false;

        // Knockback: hất lên + đẩy ra sau
        rb.linearVelocity = new Vector2(facingRight ? -knockbackHorizontal : knockbackHorizontal, knockbackVertical);

        isInvincible = true;
        invincibleTimer = invincibleTime;

        if (currentHealth <= 0f) Die();
    }

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

        // HEALTH MAIN
        if (healthFill != null)
        {
            if (healthMainHealing)
            {
                healthFill.fillAmount = Mathf.MoveTowards(healthFill.fillAmount, healthTargetRatio, mainBarHealSpeed * Time.deltaTime);
                if (Mathf.Approximately(healthFill.fillAmount, healthTargetRatio)) healthMainHealing = false;
            }
            else healthFill.fillAmount = healthTargetRatio;
        }

        // HEALTH DELAY
        if (healthDelay != null && healthFill != null)
        {
            if (healthDelay.fillAmount > healthTargetRatio && healthDelayDropping)
            {
                if (healthDelayTimer > 0f) healthDelayTimer -= Time.deltaTime;
                else
                    healthDelay.fillAmount = Mathf.MoveTowards(healthDelay.fillAmount, healthTargetRatio, delayDropSpeed * Time.deltaTime);
            }
        }

        // MANA MAIN
        if (manaFill != null)
        {
            if (manaMainHealing)
            {
                manaFill.fillAmount = Mathf.MoveTowards(manaFill.fillAmount, manaTargetRatio, manaBarHealSpeed * Time.deltaTime);
                if (Mathf.Approximately(manaFill.fillAmount, manaTargetRatio)) manaMainHealing = false;
            }
            else manaFill.fillAmount = manaTargetRatio;
        }

        // MANA DELAY
        if (manaDelay != null && manaFill != null)
        {
            if (manaDelay.fillAmount > manaTargetRatio && manaDelayDropping)
            {
                if (manaDelayTimer > 0f) manaDelayTimer -= Time.deltaTime;
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
        Debug.Log("Player died");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider != null && collision.collider.CompareTag("Enemy"))
        {
            TakeDamage(damageOnTouch);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other != null && other.CompareTag("Enemy"))
        {
            TakeDamage(damageOnTouch);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        else
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundRaycastDistance);
        }
    }
}
