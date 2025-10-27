using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum AttackDirection { Front, Back }

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runMultiplier = 1.6f;
    public float jumpForce = 12f;
    public int maxJumpCount = 2;
    private int jumpCount = 0;
    private bool isGrounded = false;

    [Header("Dash")]
    public float dashForce = 18f;
    public float dashTime = 0.18f;
    public float dashCooldown = 0.8f;
    public float dashManaCost = 25f;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private bool isDashing = false;
    private Vector2 dashDirection;

    [Header("Combat / Damage")]
    public float maxHealth = 100f;
    public float damageOnTouch = 20f;
    public float knockbackForce = 12f;
    public float knockbackAirTime = 0.6f;

    [Header("Invincibility")]
    public float invincibleTime = 0.5f;
    private bool isInvincible = false;
    private float invincibleTimer = 0f;

    [Header("Knockback Settings")]
    public float knockbackDuration = 0.3f;
    private bool isKnockbacked = false;
    private float knockbackTimer = 0f;

    [Header("Mana")]
    public float maxMana = 100f;
    public float currentMana = 100f;
    public float manaRegenRate = 12f;

    [Header("Attack")]
    public float attackCooldown = 0.25f;
    public BoxCollider2D attackHitbox;
    private bool isAttacking = false;
    private float attackCooldownTimer = 0f;
    private readonly HashSet<Collider2D> attackedEnemies = new();

    [Header("Jump Float")]
    public bool useJumpFloat = true;
    public float floatGravityScale = 0.3f;

    [Header("UI - Health")]
    public Image healthFill;
    public Image healthDelay;

    [Header("UI - Mana")]
    public Image manaFill;
    public Image manaDelay;

    [Header("Animation")]
    public Animator animator;

    [Header("Invincibility Flash")]
    public float flashFrequency = 10f;
    private readonly List<SpriteRenderer> spriteRenderers = new();
    private float flashTimer = 0f;

    [Header("UI - Dead")]
    public GameObject deadPanel;
    public float deadDelay = 2f;

    private float horizontalInput;
    private bool jumpRequested = false;
    private bool dashRequested = false;
    private bool attackRequested = false;

    public static PlayerController Instance { get; private set; }
    private Rigidbody2D rb;
    private bool facingRight = true;
    private float defaultGravityScale = 1f;
    private bool isDead = false;

    public float CurrentHealth
    {
        get => healthFill != null ? healthFill.fillAmount * maxHealth : 0f;
        set
        {
            if (healthFill != null)
            {
                float ratio = Mathf.Clamp01(value / maxHealth);
                healthFill.fillAmount = ratio;
                if (healthDelay != null) healthDelay.fillAmount = ratio;
            }
        }
    }

    void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        defaultGravityScale = rb.gravityScale;
        spriteRenderers.AddRange(GetComponentsInChildren<SpriteRenderer>(true));
    }

    void Start()
    {
        CurrentHealth = maxHealth;
        currentMana = Mathf.Clamp(currentMana, 0f, maxMana);
        UpdateUIImmediate();
    }

    void Update()
    {
        if (isDead) return;

        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (Input.GetKeyDown(KeyCode.Space)) jumpRequested = true;
        if (Input.GetKeyDown(KeyCode.Q)) dashRequested = true;
        if (Input.GetMouseButtonDown(0)) attackRequested = true;

        HandleInvincibilityFlash();
        UpdateTimers();
        RegenerateManaIfNotDashing();
        UpdateUI();
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (attackRequested && attackCooldownTimer <= 0f && !isAttacking)
        {
            StartAttack();
            attackRequested = false;
        }

        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (jumpRequested)
        {
            HandleJump();
            jumpRequested = false;
        }

        if (dashRequested)
        {
            HandleDash();
            dashRequested = false;
        }

        if (!isDashing && !isKnockbacked) HandleMovement();

        rb.gravityScale = useJumpFloat && !isGrounded && rb.linearVelocity.y < 0f
            ? (Input.GetKey(KeyCode.Space) ? floatGravityScale : defaultGravityScale)
            : defaultGravityScale;
    }

    // ===== Collision =====
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleEnemyCollision(collision);
        HandleGroundCollisionEnter(collision);
    }

    private void OnCollisionStay2D(Collision2D collision) => HandleGroundCollisionStay(collision);
    private void OnCollisionExit2D(Collision2D collision) => HandleGroundCollisionExit(collision);

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (attackHitbox != null && attackHitbox.enabled && !attackedEnemies.Contains(other))
            {
                attackedEnemies.Add(other);
                other.SendMessage("TakeDamage", damageOnTouch, SendMessageOptions.DontRequireReceiver);
            }
            else if (!isInvincible)
            {
                AttackDirection dir = GetAttackDirection(other.transform.position);
                TakeDamage(damageOnTouch, dir);
            }
        }
    }

    // ===== Helper Methods =====
    void HandleEnemyCollision(Collision2D collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            AttackDirection dir = GetAttackDirection(collision.transform.position);
            TakeDamage(damageOnTouch, dir);
        }
    }

    void HandleGroundCollisionEnter(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            jumpCount = 0;
        }
    }

    void HandleGroundCollisionStay(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) isGrounded = true;
    }

    void HandleGroundCollisionExit(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) isGrounded = false;
    }

    AttackDirection GetAttackDirection(Vector2 enemyPosition)
    {
        float playerX = transform.position.x;
        float enemyX = enemyPosition.x;
        return facingRight
            ? (enemyX >= playerX ? AttackDirection.Front : AttackDirection.Back)
            : (enemyX <= playerX ? AttackDirection.Front : AttackDirection.Back);
    }

    float CalculateKnockbackUpForce() => Mathf.Abs(Physics2D.gravity.y * rb.gravityScale) * knockbackAirTime / 2f;

    public void TakeDamage(float amount, AttackDirection direction)
    {
        if (isInvincible || isDead) return;

        CurrentHealth = Mathf.Clamp(CurrentHealth - amount, 0f, maxHealth);

        float forceMultiplier = direction == AttackDirection.Back ? 1.5f : 1f;
        float knockbackX = direction == AttackDirection.Front
            ? (facingRight ? -knockbackForce * forceMultiplier : knockbackForce * forceMultiplier)
            : (facingRight ? knockbackForce * forceMultiplier : -knockbackForce * forceMultiplier);

        float knockbackY = CalculateKnockbackUpForce();

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(knockbackX, knockbackY), ForceMode2D.Impulse);

        isKnockbacked = true;
        knockbackTimer = knockbackDuration;

        isInvincible = true;
        invincibleTimer = invincibleTime;

        if (CurrentHealth <= 0f) Die();
    }

    // ✅ Public methods để gọi từ itemSO
    public void Heal(float amount)
    {
        if (amount <= 0f || isDead) return;
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0f, maxHealth);
    }

    public void RestoreMana(float amount)
    {
        if (amount <= 0f || isDead) return;
        currentMana = Mathf.Clamp(currentMana + amount, 0f, maxMana);
    }

    void UseMana(float amount)
    {
        currentMana = Mathf.Clamp(currentMana - amount, 0f, maxMana);
    }

    void RegenerateManaIfNotDashing()
    {
        if (!isDashing)
            currentMana = Mathf.Clamp(currentMana + manaRegenRate * Time.deltaTime, 0f, maxMana);
    }

    void UpdateUI()
    {
        if (healthFill) healthFill.fillAmount = CurrentHealth / maxHealth;
        if (manaFill) manaFill.fillAmount = currentMana / maxMana;
    }

    void UpdateUIImmediate() => UpdateUI();

    void UpdateAnimation()
    {
        if (animator == null || isDead) return;

        if (isKnockbacked) animator.Play("Hurt");
        else if (isDashing) animator.Play("Dash");
        else if (isAttacking) return;
        else if (!isGrounded) animator.Play(rb.linearVelocity.y > 0 ? "Jump" : "Fall");
        else animator.Play(Mathf.Abs(horizontalInput) > 0.1f ? "Run" : "Idle");
    }

    void Flip()
    {
        facingRight = !facingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    void HandleJump()
    {
        if (jumpCount < maxJumpCount)
        {
            if (isGrounded) rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpCount++;
            isGrounded = false;
        }
    }

    void HandleDash()
    {
        if (dashCooldownTimer <= 0f && currentMana >= dashManaCost)
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
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        rb.AddForce(dashDirection * dashForce, ForceMode2D.Impulse);
    }

    void EndDash() => isDashing = false;

    void StartAttack()
    {
        isAttacking = true;
        if (animator) animator.Play("Attack");
        attackedEnemies.Clear();
        if (attackHitbox) attackHitbox.enabled = true;
        attackCooldownTimer = attackCooldown;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        CurrentHealth = 0f;
        rb.linearVelocity = Vector2.zero;

        if (animator) animator.Play("chet");
        if (deadPanel) deadPanel.SetActive(true);

        Invoke(nameof(Respawn), deadDelay);
    }

    void Respawn()
    {
        if (deadPanel) deadPanel.SetActive(false);
        isDead = false;
        CurrentHealth = maxHealth;
        currentMana = maxMana;
        UpdateUI();
    }

    void HandleMovement()
    {
        if (isKnockbacked) return;

        float moveSpeed = walkSpeed * (Input.GetKey(KeyCode.LeftShift) ? runMultiplier : 1f);
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        if (horizontalInput > 0 && !facingRight) Flip();
        else if (horizontalInput < 0 && facingRight) Flip();
    }

    void UpdateTimers()
    {
        if (isKnockbacked)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f) isKnockbacked = false;
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f) EndDash();
        }

        if (dashCooldownTimer > 0f) dashCooldownTimer -= Time.deltaTime;
        if (invincibleTimer > 0f) invincibleTimer -= Time.deltaTime;
        if (attackCooldownTimer > 0f) attackCooldownTimer -= Time.deltaTime;
    }

    void HandleInvincibilityFlash()
    {
        if (!isInvincible || spriteRenderers.Count == 0) return;

        flashTimer += Time.deltaTime * flashFrequency;
        bool visible = Mathf.FloorToInt(flashTimer) % 2 == 0;
        foreach (var sr in spriteRenderers) sr.enabled = visible;
    }
}
