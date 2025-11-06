using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [Header("Last Stand Willpower")]
    public bool enableWillpowerRegen = true;
    public float willpowerMultiplier = 2f;
    public float willpowerThreshold = 0.5f;

    // ====== Attack ======
    [Header("Attack")]
    public float attackCooldown = 0.25f;
    public BoxCollider2D attackHitbox;

    private bool isAttacking = false;
    private float attackCooldownTimer = 0f;
    private HashSet<Collider2D> attackedEnemies = new HashSet<Collider2D>();

    // ====== Jump Float ======
    [Header("Jump Float")]
    public bool useJumpFloat = true;
    public float floatGravityScale = 0.3f;

    // ====== UI ======
    [Header("UI - Health")]
    public Image healthFill;      // Thanh máu chính (giảm ngay)
    public Image healthDelay;     // Thanh delay (giảm chậm)

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

    [Header("Game Fade")]
    public Image gameFadePanel;
    public float fadeDuration = 1f;

    // ====== Animation ======
    [Header("Animation")]
    public Animator animator;

    // ====== Invincibility Flash ======
    [Header("Invincibility Flash")]
    public float flashFrequency = 10f;
    private List<SpriteRenderer> spriteRenderers;
    private float flashTimer = 0f;

    // ====== Input Buffering ======
    private float horizontalInput;
    private bool jumpRequested = false;
    private bool dashRequested = false;
    private bool attackRequested = false;

    // ====== Internal ======
    public static PlayerController Instance;
    private Rigidbody2D rb;
    private bool facingRight = true;
    private float defaultGravityScale = 1f;
    public bool isDead = false;

    // ====== Health Delay Internal ======
    private float healthDelayTimer = 0f;
    private bool healthDelayDropping = false;
    private bool healthMainHealing = false;

    // ====== Mana Internal ======
    private float manaTargetRatio;
    private float manaDelayTimer = 0f;
    private bool manaDelayDropping = false;
    private bool manaMainHealing = false;

    // ====== HEALTH: CHỈ DÙNG healthFill LÀM NGUỒN CHÂN THẬT ======
    private const float HEALTH_EPSILON = 0.001f;

    public float CurrentHealth
    {
        get
        {
            if (healthFill == null) return 0f;
            float value = healthFill.fillAmount * maxHealth;
            return value < HEALTH_EPSILON ? 0f : value;
        }
        set
        {
            if (healthFill == null) return;
            if (value < HEALTH_EPSILON) value = 0f;
            float ratio = Mathf.Clamp01(value / maxHealth);
            healthFill.fillAmount = ratio;
        }
    }

    void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        defaultGravityScale = rb.gravityScale;
        spriteRenderers = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>(true));

        if (rb == null)
            Debug.LogError("Rigidbody2D not found on Player!");
        if (spriteRenderers.Count == 0)
            Debug.LogWarning("No SpriteRenderers found on Player or its children!");
    }

    void Start()
    {
        CurrentHealth = maxHealth;
        currentMana = Mathf.Clamp(currentMana, 0f, maxMana);
        manaTargetRatio = currentMana / maxMana;
        UpdateUIImmediate();

        // Kiểm tra respawn từ checkpoint
        if (PlayerPrefs.GetInt("HasRespawnPos", 0) == 1)
        {
            float x = PlayerPrefs.GetFloat("RespawnX");
            float y = PlayerPrefs.GetFloat("RespawnY");
            float z = PlayerPrefs.GetFloat("RespawnZ");
            transform.position = new Vector3(x, y, z);
            PlayerPrefs.SetInt("HasRespawnPos", 0);
        }

        if (gameFadePanel != null)
        {
            StartCoroutine(FadePanel(1f, 0f));
        }
    }

    void Update()
    {
        // Luôn cập nhật UI và hiệu ứng (kể cả khi chết)
        HandleInvincibilityFlash();
        UpdateTimers();
        RegenerateManaIfNotDashing();
        UpdateUI();
        UpdateAnimation();

        // Chỉ xử lý input nếu chưa chết
        if (isDead) return;

        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (Input.GetKeyDown(KeyCode.Space)) jumpRequested = true;
        if (Input.GetKeyDown(KeyCode.Q)) dashRequested = true;
        if (Input.GetMouseButtonDown(0)) attackRequested = true;
    }

    void FixedUpdate()
    {
        if (isDead)
        {
            rb.simulated = false;
            return;
        }

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

        if (!isDashing && !isKnockbacked)
        {
            HandleMovement();
        }

        if (useJumpFloat && !isGrounded && rb.linearVelocity.y < 0f)
        {
            rb.gravityScale = Input.GetKey(KeyCode.Space) ? floatGravityScale : defaultGravityScale;
        }
        else
        {
            rb.gravityScale = defaultGravityScale;
        }
    }

    // ==============================
    // CÁC HÀM CHÍNH (GIỮ NGUYÊN)
    // ==============================

    void HandleInvincibilityFlash()
    {
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
    }

    void UpdateTimers()
    {
        if (isKnockbacked)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f) isKnockbacked = false;
        }

        if (dashCooldownTimer > 0f) dashCooldownTimer -= Time.deltaTime;
        if (dashTimer > 0f) dashTimer -= Time.deltaTime;
        else if (isDashing) EndDash();

        if (attackCooldownTimer > 0f) attackCooldownTimer -= Time.deltaTime;
    }

    void HandleJump()
    {
        if (jumpCount < maxJumpCount)
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            }
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

    void HandleMovement()
    {
        float speed = walkSpeed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            speed *= runMultiplier;

        rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);

        if (horizontalInput > 0.01f && !facingRight) Flip();
        else if (horizontalInput < -0.01f && facingRight) Flip();
    }

    void Flip()
    {
        facingRight = !facingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
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

    void StartAttack()
    {
        isAttacking = true;

        if (animator != null)
            animator.Play("Attack");

        attackedEnemies.Clear();

        if (attackHitbox != null)
        {
            attackHitbox.enabled = true;
            StartCoroutine(DisableHitboxAndAttackAfterDelay(GetAnimationLength("Attack")));
        }

        attackCooldownTimer = attackCooldown;
    }

    System.Collections.IEnumerator DisableHitboxAndAttackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (attackHitbox != null)
            attackHitbox.enabled = false;
        isAttacking = false;
    }

    float GetAnimationLength(string animName)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return 0.3f;

        var clip = animator.runtimeAnimatorController.animationClips
            .FirstOrDefault(c => c.name == animName);
        return clip != null ? clip.length : 0.3f;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleEnemyCollision(collision);
        HandleGroundCollisionEnter(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleGroundCollisionStay(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        HandleGroundCollisionExit(collision);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null || !other.CompareTag("Enemy")) return;

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

    void HandleEnemyCollision(Collision2D collision)
    {
        if (collision.collider != null && collision.collider.CompareTag("Enemy"))
        {
            AttackDirection dir = GetAttackDirection(collision.transform.position);
            TakeDamage(damageOnTouch, dir);
        }
    }

    void HandleGroundCollisionEnter(Collision2D collision)
    {
        if (!isKnockbacked && collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            jumpCount = 0;
        }
    }

    void HandleGroundCollisionStay(Collision2D collision)
    {
        if (!isKnockbacked && collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void HandleGroundCollisionExit(Collision2D collision)
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

    float CalculateKnockbackUpForce()
    {
        float gravityMagnitude = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
        return (gravityMagnitude * knockbackAirTime) / 2f;
    }

    public void TakeDamage(float amount, AttackDirection direction)
    {
        if (isInvincible || isDead) return;

        float newHealth = CurrentHealth - amount;
        CurrentHealth = newHealth;

        if (healthDelay != null)
        {
            healthDelayTimer = delayBeforeDrop;
            healthDelayDropping = true;
            healthMainHealing = false;
        }

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

        Debug.Log($"Bị đánh! Health: {CurrentHealth:F6}");

        if (CurrentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || isDead) return;

        float newHealth = Mathf.Clamp(CurrentHealth + amount, 0f, maxHealth);
        CurrentHealth = newHealth;

        if (healthDelay != null)
        {
            float newDelayRatio = newHealth / maxHealth;
            if (newDelayRatio > healthDelay.fillAmount)
            {
                healthDelay.fillAmount = newDelayRatio;
            }
            healthMainHealing = true;
            healthDelayDropping = false;
            healthDelayTimer = 0f;
        }
    }

    void UseMana(float amount)
    {
        if (amount <= 0f || isDead) return;
        currentMana = Mathf.Clamp(currentMana - amount, 0f, maxMana);
        manaTargetRatio = currentMana / maxMana;

        if (manaFill != null) manaFill.fillAmount = manaTargetRatio;

        manaDelayTimer = manaDelayBeforeDrop;
        manaDelayDropping = true;
        manaMainHealing = false;
    }

    public void RestoreMana(float amount)
    {
        if (amount <= 0f || isDead) return;
        currentMana = Mathf.Clamp(currentMana + amount, 0f, maxMana);
        manaTargetRatio = currentMana / maxMana;

        if (manaDelay != null) manaDelay.fillAmount = manaTargetRatio;

        manaMainHealing = true;
        manaDelayDropping = false;
        manaDelayTimer = 0f;
    }

    void RegenerateManaIfNotDashing()
    {
        if (isDashing || isDead) return;

        float regenRate = manaRegenRate;

        if (enableWillpowerRegen)
        {
            float healthRatio = CurrentHealth / maxHealth;
            if (healthRatio <= willpowerThreshold)
            {
                float intensity = Mathf.InverseLerp(willpowerThreshold, 0f, healthRatio);
                float willpowerFactor = 1f + willpowerMultiplier * intensity;
                regenRate *= willpowerFactor;
            }
        }

        RegenerateMana(Time.deltaTime * regenRate);
    }

    void RegenerateMana(float amount)
    {
        if (amount <= 0f || isDead) return;
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
        // Cập nhật mana (chỉ khi còn sống)
        if (!isDead)
        {
            manaTargetRatio = currentMana / maxMana;
            UpdateManaBar();
            UpdateManaDelayBar();
        }

        // Cập nhật máu — LUÔN CHẠY (kể cả khi chết)
        if (healthDelay != null && healthFill != null)
        {
            if (isDead)
            {
                // Khi chết: healthFill = 0, healthDelay giảm về 0
                healthFill.fillAmount = 0f;
                if (healthDelay.fillAmount > 0f)
                {
                    healthDelay.fillAmount = Mathf.MoveTowards(
                        healthDelay.fillAmount,
                        0f,
                        delayDropSpeed * Time.deltaTime
                    );
                }
            }
            else
            {
                // Khi còn sống: logic delay bình thường
                if (healthDelayDropping)
                {
                    if (healthDelayTimer > 0f)
                    {
                        healthDelayTimer -= Time.deltaTime;
                    }
                    else
                    {
                        healthDelay.fillAmount = Mathf.MoveTowards(
                            healthDelay.fillAmount,
                            healthFill.fillAmount,
                            delayDropSpeed * Time.deltaTime
                        );
                        if (Mathf.Approximately(healthDelay.fillAmount, healthFill.fillAmount))
                        {
                            healthDelayDropping = false;
                        }
                    }
                }
                else if (healthMainHealing)
                {
                    healthFill.fillAmount = Mathf.MoveTowards(
                        healthFill.fillAmount,
                        healthDelay.fillAmount,
                        mainBarHealSpeed * Time.deltaTime
                    );
                    if (Mathf.Approximately(healthFill.fillAmount, healthDelay.fillAmount))
                    {
                        healthMainHealing = false;
                    }
                }
            }
        }

        // Gọi Die() nếu máu về 0 (chỉ khi còn sống)
        if (!isDead && CurrentHealth <= 0f)
        {
            Die();
        }
    }

    void UpdateManaBar()
    {
        if (manaFill == null) return;

        if (manaMainHealing)
        {
            manaFill.fillAmount = Mathf.MoveTowards(manaFill.fillAmount, manaTargetRatio, manaBarHealSpeed * Time.deltaTime);
            if (Mathf.Approximately(manaFill.fillAmount, manaTargetRatio))
                manaMainHealing = false;
        }
        else
        {
            manaFill.fillAmount = manaTargetRatio;
        }
    }

    void UpdateManaDelayBar()
    {
        if (manaDelay == null || manaFill == null) return;

        if (manaDelay.fillAmount > manaTargetRatio && manaDelayDropping)
        {
            if (manaDelayTimer > 0f)
                manaDelayTimer -= Time.deltaTime;
            else
                manaDelay.fillAmount = Mathf.MoveTowards(manaDelay.fillAmount, manaTargetRatio, manaDelayDropSpeed * Time.deltaTime);
        }
    }

    void UpdateUIImmediate()
    {
        float healthRatio = CurrentHealth / maxHealth;
        float manaRatio = currentMana / maxMana;

        if (healthFill != null) healthFill.fillAmount = healthRatio;
        if (healthDelay != null) healthDelay.fillAmount = healthRatio;

        if (manaFill != null) manaFill.fillAmount = manaRatio;
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
        if (isDead) return;
        isDead = true;

        CurrentHealth = 0f;

        isInvincible = false;
        isAttacking = false;
        isKnockbacked = false;
        rb.simulated = false;

        foreach (var sr in spriteRenderers)
        {
            sr.enabled = true;
        }

        if (animator != null)
        {
            animator.Play("chet");
            StartCoroutine(RespawnAfterDeath());
        }
        else
        {
            RespawnImmediately();
        }
    }

    System.Collections.IEnumerator RespawnAfterDeath()
    {
        rb.simulated = false;

        float deathAnimLength = GetAnimationLength("chet");
        if (deathAnimLength <= 0) deathAnimLength = 1f;
        yield return new WaitForSeconds(deathAnimLength);

        // Đợi healthDelay về 0
        while (healthDelay != null && healthDelay.fillAmount > 0.01f)
        {
            yield return null;
        }

        if (gameFadePanel != null)
        {
            gameFadePanel.gameObject.SetActive(true);
            yield return StartCoroutine(FadePanel(0f, 1f));
        }

        enabled = false; // Tắt ngay trước khi load scene
        PerformRespawn();
    }

    void RespawnImmediately()
    {
        rb.simulated = false;

        // Đợi healthDelay về 0 (nếu có)
        while (healthDelay != null && healthDelay.fillAmount > 0.01f)
        {
            // Không thể dùng while trong void, nên dùng coroutine
            // Nhưng vì RespawnImmediately ít dùng, ta bỏ qua hoặc gọi coroutine
        }

        enabled = false;
        PerformRespawn();
    }

    void PerformRespawn()
    {
        string targetScene = "Scene-02";
        Vector3 spawnPosition = Vector3.zero;
        bool foundCheckpoint = false;

        // === ƯU TIÊN CHECKPOINT TỪ PLAYERPREFS ===
        if (PlayerPrefs.HasKey("LastCheckpointScene"))
        {
            targetScene = PlayerPrefs.GetString("LastCheckpointScene");
            spawnPosition = new Vector3(
                PlayerPrefs.GetFloat("CheckpointX"),
                PlayerPrefs.GetFloat("CheckpointY"),
                PlayerPrefs.GetFloat("CheckpointZ")
            );
            foundCheckpoint = true;
        }
        else
        {
            // Nếu không có checkpoint, thử load từ SaveData
            for (int slotIndex = 0; slotIndex < 3; slotIndex++)
            {
                SaveData saveData = SaveSystem.LoadGame(slotIndex);
                if (saveData != null && saveData.scenes != null && saveData.scenes.Count > 0)
                {
                    SceneSaveData latest = saveData.scenes[saveData.scenes.Count - 1];
                    targetScene = latest.sceneName;
                    spawnPosition = latest.position;
                    foundCheckpoint = true;
                    PlayerPrefs.SetInt("CurrentSlot", slotIndex);
                    break;
                }
            }
        }

        PlayerPrefs.SetString("RespawnScene", targetScene);
        PlayerPrefs.SetFloat("RespawnX", spawnPosition.x);
        PlayerPrefs.SetFloat("RespawnY", spawnPosition.y);
        PlayerPrefs.SetFloat("RespawnZ", spawnPosition.z);
        PlayerPrefs.SetInt("HasRespawnPos", foundCheckpoint ? 1 : 0);

        SceneManager.LoadScene(targetScene);
    }

    System.Collections.IEnumerator FadePanel(float startAlpha, float endAlpha)
    {
        if (gameFadePanel == null) yield break;

        gameFadePanel.gameObject.SetActive(true);
        Color color = gameFadePanel.color;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            gameFadePanel.color = new Color(color.r, color.g, color.b, alpha);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        gameFadePanel.color = new Color(color.r, color.g, color.b, endAlpha);

        if (endAlpha <= 0f)
        {
            gameFadePanel.gameObject.SetActive(false);
        }
    }

    void UpdateAnimation()
    {
        if (animator == null || isDead) return;

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

        if (isAttacking)
        {
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
}