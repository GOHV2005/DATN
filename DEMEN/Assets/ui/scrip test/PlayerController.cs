using System.Collections;
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
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runMultiplier = 1.6f;
    public float jumpForce = 12f;
    public int maxJumpCount = 2;

    private int jumpCount = 0;
    private bool isGrounded = false;

    [Header("Ground Check")]
    public Transform groundCheck;          // 👈 Gán điểm kiểm tra dưới chân
    public float groundCheckRadius = 0.2f; // Bán kính kiểm tra
    public LayerMask groundLayer;          // 👈 Hoặc dùng Tag (xem dưới)
    public string groundTag = "Ground";    // 👈 DÙNG TAG NHƯ BẠN MUỐN

    [Header("Dash")]
    public float dashForce = 18f;
    public float dashTime = 0.18f;
    public float dashCooldown = 0.8f;
    public float dashManaCost = 25f;

    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private bool isDashing = false;
    private Vector2 dashDirection;
    private float dashAnimationDuration = 0.3f;
    private float dashAnimationTimer = 0f;
    public GameObject dashSmokePrefab;
    public Transform targetObject;

    [Header("Combat")]
    public float maxHealth = 100f;
    public float damageOnTouch = 20f;

    [Header("Knockback")]
    public float knockbackForce = 12f;
    public float knockbackAirTime = 0.6f;

    [Header("Invincibility")]
    public float invincibleTime = 0.5f;

    [Header("Dash Invincibility")]
    private bool isDashInvincible = false; // 👈 BIẾN MỚI
    private float dashInvincibleTimer = 2f; // 👈 BIẾN MỚI

    [Header("Knockback Invincibility")]
    private bool isKnockbackInvincible = false; // 👈 BIẾN MỚI
    private float knockbackInvincibleTimer = 0f; // 👈 BIẾN MỚI

    [Header("Knockback Settings")]
    public float knockbackDuration = 0.3f;
    private bool isKnockbacked = false;
    private float knockbackTimer = 0f;

    [Header("Mana")]
    public float maxMana = 100f;
    public float currentMana = 100f;
    public float manaRegenRate = 12f;
    public GameObject manaShardPrefab;
    public Transform manaBarPosition;

    [Header("Last Stand Willpower")]
    public bool enableWillpowerRegen = true;
    public float willpowerMultiplier = 2f;
    public float willpowerThreshold = 0.5f;

    [Header("Attack")]
    public float attackCooldown = 0.25f;
    public BoxCollider2D attackHitbox;
    private bool isTakingDamage = false;
    private bool isAttacking = false;
    private float attackCooldownTimer = 0f;
    private HashSet<Collider2D> attackedEnemies = new HashSet<Collider2D>();

    [Header("Longden Equipment")]
    public GameObject longdenObject; // Kéo GameObject longden vào đây (đã có sẵn trong scene)


    private bool isEquippingLongden = false;
    public bool IsHoldingLongden { get; private set; } = false;
    private bool longdenJustUnequipped = false;
    public void MarkLongdenAsJustUnequipped() => longdenJustUnequipped = true;
    public bool justUnequippedLongden = false;

    [Header("Cuoc Chim")]
    public GameObject cuocChimObject;           // 👈 Đã có
    public PolygonCollider2D cuocChimHitbox;    // 👈 Collider dùng để gây dame
    private bool isUsingCuocChim = false;       // đang trong animation đập
    public float cuocChimDamage = 30f;          // hoặc lấy từ itemSO nếu muốn
    private bool isEquippingCuocChim = false; // 👈 mới
    private bool isCuocChimVisible = false; // true khi đang hiện, false khi ẩn
    private bool cuocChimJustUnequipped = false;
    public void MarkCuocChimAsJustUnequipped() => cuocChimJustUnequipped = true;
    public bool justUnequippedCuocChim = false;

    public bool IsHoldingCuocChim { get; private set; } = false;
    [Header("Jump Float")]
    public bool useJumpFloat = true;
    public float floatGravityScale = 0.3f;

    [Header("UI - Health (Heart System)")]
    public Image[] heartImages;
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;
    public float healthPerHeart = 20f;
    public System.Action onTakeDamage; // 👈 THÊM DÒNG NÀY
    [Header("UI - Mana")]
    public Image manaFill;

    [Header("UI Settings - Mana")]
    public float manaBarHealSpeed = 0.9f;

    [Header("Game Fade")]
    public Image gameFadePanel;
    public float fadeDuration = 1f;

    [Header("Animation")]
    public Animator animator;

    [Header("Invincibility Flash")]
    public float flashFrequency = 10f;
    private List<SpriteRenderer> spriteRenderers;
    private float flashTimer = 0f;

    [Header("Drop Settings")]
    public Transform dropPoint;
    public Transform feetPoint;
    public float dropForce = 8f;
    public float dropAngle = 50f;

    private float horizontalInput;
    private bool jumpRequested = false;
    private bool dashRequested = false;
    private bool attackRequested = false;
    private bool isDropping = false;
    private System.Action dropOnComplete;

    public static PlayerController Instance;
    private Rigidbody2D rb;
    private bool facingRight = true;
    private float defaultGravityScale = 1f;
    public bool isDead = false;

    private float currentHealth;
    public float CurrentHealth
    {
        get => currentHealth;
        set
        {
            currentHealth = Mathf.Clamp(value, 0f, maxHealth);
            UpdateHeartUI();
        }
    }

    private float manaTargetRatio;
    private bool manaMainHealing = false;

    void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        defaultGravityScale = rb.gravityScale;
        spriteRenderers = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>(true));
    }

    void Start()
    {
        CurrentHealth = maxHealth;
        currentMana = Mathf.Clamp(currentMana, 0f, maxMana);
        manaTargetRatio = currentMana / maxMana;

        if (manaFill != null) manaFill.fillAmount = manaTargetRatio;

        int currentSlot = PlayerPrefs.GetInt("CurrentSlot", 0);
        SaveData saveData = SaveSystem.LoadGame(currentSlot);

        if (saveData != null && saveData.scenes != null && saveData.scenes.Count > 0)
        {
            SceneSaveData latest = saveData.scenes[saveData.scenes.Count - 1];
            if (latest.sceneName == SceneManager.GetActiveScene().name)
            {
                transform.position = latest.position;
                Debug.Log($"[Respawn] Loaded player position from save slot {currentSlot}: {latest.position}");
            }
        }

        if (gameFadePanel != null)
        {
            StartCoroutine(FadePanel(1f, 0f));
        }
    }

    void Update()
    {
        HandleInvincibilityFlash();
        UpdateTimers();
        RegenerateManaIfNotDashing();
        UpdateManaUI();
        UpdateAnimation();
        CheckGround();

        if (isDead) return;
        if (IsHoldingCuocChim && Input.GetKeyDown(KeyCode.E) && !isUsingCuocChim && !isAttacking && !isDashing && !isDead)
        {
            
            Collider2D[] hits = Physics2D.OverlapCircleAll(feetPoint.position, 1.2f);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("rock") || hit.CompareTag("Enemy"))
                {
                    UseCuocChimOnTarget(hit.transform);
                    break;
                }
            }
        }
        if (UIManager.IsGameplayInputAllowed)
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            if (Input.GetKeyDown(KeyCode.Space)) jumpRequested = true;
            if (Input.GetKeyDown(KeyCode.Q)) dashRequested = true;
            if (Input.GetMouseButtonDown(0)) attackRequested = true;
        }
        else
        {
            horizontalInput = 0f;
        }
    }
    void CheckGround()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius);
        isGrounded = false;
        jumpCount = 0;

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag(groundTag))
            {
                isGrounded = true;
                jumpCount = 0; // Reset jump khi chạm đất
                break;
            }
        }
    }

    void FixedUpdate()
    {
        if (isDead) { rb.simulated = false; return; }

        if (isDropping)
        {
            bool shouldCancel =
                Mathf.Abs(horizontalInput) > 0.1f ||
                jumpRequested ||
                dashRequested ||
                attackRequested ||
                isKnockbacked ||
                isAttacking ||
                isDashing;

            if (shouldCancel)
            {
                CancelDrop();
            }
        }

        if (isDead) { rb.simulated = false; return; }

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

        if (isDashing) return;

        if (useJumpFloat && !isGrounded && rb.linearVelocity.y < 0f)
        {
            rb.gravityScale = Input.GetKey(KeyCode.Space) ? floatGravityScale : defaultGravityScale;
        }
        else
        {
            rb.gravityScale = defaultGravityScale;
        }
    }

    public void DropItem(string name, int qty, Sprite sprite, string desc, System.Action onComplete = null)
    {
        CancelEquippingActions();
        if (animator == null || isDead) return;

        CancelDrop();

        isDropping = true;
        dropOnComplete = onComplete;

        animator.Play("dropItem");
        StartCoroutine(DelayedDropSpawn(name));
    }

    private IEnumerator DelayedDropSpawn(string itemName)
    {
        yield return new WaitForSeconds(1.15f);

        if (!isDropping) yield break;

        if (dropPoint == null) { CancelDrop(); yield break; }

        InventoryManager invMgr = InventoryManager.Instance;
        GameObject prefab = invMgr?.GetItemPrefab(itemName);

        if (prefab == null) { CancelDrop(); yield break; }

        GameObject itemObj = Instantiate(prefab, dropPoint.position, Quaternion.identity);
        Rigidbody2D rb = itemObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            float dir = facingRight ? 1f : -1f;
            float rad = Mathf.Deg2Rad * dropAngle;
            Vector2 force = new Vector2(Mathf.Cos(rad) * dir, Mathf.Sin(rad)) * dropForce;
            rb.AddForce(force, ForceMode2D.Impulse);
        }

        dropOnComplete?.Invoke();
        isDropping = false;
        dropOnComplete = null;
    }

    private void CancelDrop()
    {
        if (!isDropping) return;

        isDropping = false;
        dropOnComplete = null;

        if (animator != null && !isDead)
        {
            animator.Play("Idle");
        }
    }

    void UpdateHeartUI()
    {
        if (heartImages == null || heartImages.Length == 0) return;

        int maxHearts = Mathf.CeilToInt(maxHealth / healthPerHeart);
        float currentHealth = CurrentHealth;

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i >= maxHearts)
            {
                heartImages[i].gameObject.SetActive(false);
                continue;
            }

            heartImages[i].gameObject.SetActive(true);
            float healthEnd = (i + 1) * healthPerHeart;
            heartImages[i].sprite = (currentHealth >= healthEnd) ? fullHeartSprite : emptyHeartSprite;
        }
    }

    void HandleInvincibilityFlash()
    {
        // 👇 CHỈ CHỚP KHI BỊ ĐÁNH (KNOCKBACK INVINCIBLE)
        if (isKnockbackInvincible)
        {
            knockbackInvincibleTimer -= Time.deltaTime;
            if (knockbackInvincibleTimer <= 0f) isKnockbackInvincible = false;

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
        }
        else
        {
            // 👇 LUÔN BẬT SPRITE LẠI KHI KHÔNG CHỚP
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

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        if (dashTimer > 0f)
        {
            dashTimer -= Time.deltaTime;
        }
        else if (isDashing)
        {
            EndDash();
        }

        if (attackCooldownTimer > 0f) attackCooldownTimer -= Time.deltaTime;
    }

    void HandleJump()
    {
        CancelEquippingActions();
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
        Debug.Log($"[DASH] Requested! Cooldown: {dashCooldownTimer:F2}, Mana: {currentMana}/{maxMana}, IsDashing: {isDashing}");

        if (dashCooldownTimer <= 0f && currentMana >= dashManaCost)
        {
            Vector2 dir = Mathf.Abs(horizontalInput) > 0.1f
                ? new Vector2(Mathf.Sign(horizontalInput), 0f)
                : (facingRight ? Vector2.right : Vector2.left);
            StartDash(dir.normalized);
        }
        else
        {
            if (dashCooldownTimer > 0f)
                Debug.Log("[DASH] ❌ Bị cooldown!");
            if (currentMana < dashManaCost)
                Debug.Log("[DASH] ❌ Thiếu mana!");
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
        CancelEquippingActions();
        UseMana(dashManaCost);
        isDashing = true;
        dashDirection = dir;
        dashTimer = dashTime;
        dashCooldownTimer = dashCooldown;
        dashAnimationTimer = dashAnimationDuration;

        isDashInvincible = true;
        dashInvincibleTimer = dashTime;

        // 👇 TẮT VA CHẠM VỚI ENEMY TRONG LÚC DASH
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), true);

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        rb.AddForce(dashDirection * dashForce, ForceMode2D.Impulse);

        if (animator != null) animator.Play("Dash");
        if (dashSmokePrefab != null && targetObject != null)
        {
            GameObject smoke = Instantiate(dashSmokePrefab);
            smoke.GetComponent<AutoDestroyAfterAnim>().Init(targetObject.position, facingRight);
        }
    }

    void EndDash()
    {
        isDashing = false;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        isDashInvincible = false;

        // 👇 BẬT LẠI VA CHẠM SAU KHI DASH
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), false);
    }

    void StartAttack()
    {
        if (isDead) return;

        isAttacking = true;
        CancelEquippingActions();
        // 👇 DÙNG ANIMATION PHÙ HỢP
        string attackAnim = IsHoldingCuocChim ? "sudungcuocchim" : "Attack";
        animator.Play(attackAnim);

        attackedEnemies.Clear();

        // 👇 BẬT HITBOX PHÙ HỢP
        Collider2D hitbox = IsHoldingCuocChim ? (Collider2D)cuocChimHitbox : attackHitbox;
        if (hitbox != null)
        {
            hitbox.enabled = true;
            StartCoroutine(DisableHitboxAfterDelay(GetAnimationLength(attackAnim), hitbox));
        }

        attackCooldownTimer = attackCooldown;
    }
    IEnumerator DisableHitboxAfterDelay(float delay, Collider2D hitbox)
    {
        yield return new WaitForSeconds(delay);
        if (hitbox != null) hitbox.enabled = false;
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("void"))
        {
            TakeDamageFromEnemy(maxHealth, other.transform.position);
            return;
        }

        // 🪓 CUỐC CHIM: Only activates if cuocChimHitbox is enabled
        if (cuocChimHitbox != null && cuocChimHitbox.enabled)
        {
            Health targetHealth = other.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(cuocChimDamage);

                // Special handling for rocks (if needed)
                if (other.CompareTag("rock"))
                {
                    targetHealth.onDeath += OnRockDestroyed;
                }
            }
            return; // Stop further checks while cuốc chim is active
        }

        // ⚔️ NORMAL ATTACK: Only if attackHitbox is enabled and enemy not already hit
        if (attackHitbox != null && attackHitbox.enabled)
        {
            Health enemyHealth = other.GetComponent<Health>();
            if (enemyHealth != null && !attackedEnemies.Contains(other))
            {
                attackedEnemies.Add(other);
                enemyHealth.TakeDamage(damageOnTouch);
            }
            return;
        }

        // ====== PLAYER TAKES DAMAGE ======
        if (other.CompareTag("Enemy") && !isDashInvincible && !isKnockbackInvincible && !isAttacking)
        {
            AttackDirection dir = GetAttackDirection(other.transform.position);
            TakeDamage(damageOnTouch, dir);
        }
    }
    private void OnRockDestroyed()
    {
        // 1. Hủy trang bị cuốc chim
        if (IsHoldingCuocChim)
        {
            // Ẩn object
            cuocChimObject?.SetActive(false);
            IsHoldingCuocChim = false;

            // 2. Trừ 1 item "cuốc chim" trong inventory
            InventoryManager.Instance?.RemoveItem("cuốc chim", 1);
        }
    }
    void HandleEnemyCollision(Collision2D collision)
    {
        /*if (collision.collider != null && collision.collider.CompareTag("Enemy"))
        {
            // 👇 THÊM KIỂM TRA isDashInvincible và isKnockbackInvincible
            if (!isDashInvincible && !isKnockbackInvincible)
            {
                AttackDirection dir = GetAttackDirection(collision.transform.position);
                TakeDamage(damageOnTouch, dir);
            }
        }*/
    }

    void OnDrawGizmos()
    {
        if (groundCheck == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    public AttackDirection GetAttackDirection(Vector2 enemyPosition)
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
        // 🔒 Prevent re-entrancy (StackOverflow fix)
        if (isTakingDamage || isDashInvincible || isKnockbackInvincible || isDead)
            return;

        isTakingDamage = true;
        try
        {
            CancelEquippingActions();
            if (isDropping) CancelDrop();

            CurrentHealth -= amount;

            float forceMultiplier = (direction == AttackDirection.Back) ? 1.5f : 1f;
            float knockbackX = (direction == AttackDirection.Front)
                ? (facingRight ? -knockbackForce * forceMultiplier : knockbackForce * forceMultiplier)
                : (facingRight ? knockbackForce * forceMultiplier : -knockbackForce * forceMultiplier);

            float knockbackY = CalculateKnockbackUpForce();

            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(knockbackX, knockbackY), ForceMode2D.Impulse);

            isKnockbacked = true;
            knockbackTimer = knockbackDuration;

            isKnockbackInvincible = true;
            knockbackInvincibleTimer = invincibleTime;

            onTakeDamage?.Invoke();

            if (CurrentHealth <= 0f)
            {
                Die();
            }
        }
        finally
        {
            isTakingDamage = false;
        }
    }

    public void TakeDamageFromEnemy(float amount, Vector2 enemyPosition)
    {
        if (isTakingDamage || isDashInvincible || isKnockbackInvincible || isDead) return;
        if (isDashing) return;

        CancelEquippingActions();
        if (isDropping) CancelDrop();

        CurrentHealth -= amount;

        AttackDirection dir = GetAttackDirection(enemyPosition);
        float forceMultiplier = (dir == AttackDirection.Back) ? 1.5f : 1f;
        float knockbackX = (dir == AttackDirection.Front)
            ? (facingRight ? -knockbackForce * forceMultiplier : knockbackForce * forceMultiplier)
            : (facingRight ? knockbackForce * forceMultiplier : -knockbackForce * forceMultiplier);

        float knockbackY = CalculateKnockbackUpForce();

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(knockbackX, knockbackY), ForceMode2D.Impulse);

        isKnockbacked = true;
        knockbackTimer = knockbackDuration;

        isKnockbackInvincible = true;
        knockbackInvincibleTimer = invincibleTime;

        onTakeDamage?.Invoke();

        if (CurrentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || isDead) return;
        CurrentHealth += amount;
    }

    public void UseMana(float amount)
    {
        if (amount <= 0 || isDead) return;

        float prevMana = currentMana;
        currentMana = Mathf.Clamp(currentMana - amount, 0f, maxMana);

        if (manaFill != null) manaFill.fillAmount = currentMana / maxMana;

        if (currentMana < prevMana && manaShardPrefab != null && manaBarPosition != null)
        {
            int shardCount = Mathf.CeilToInt(amount / 4f);
            shardCount = Mathf.Clamp(shardCount, 1, 8);

            for (int i = 0; i < shardCount; i++)
            {
                GameObject shard = Instantiate(manaShardPrefab, manaBarPosition);

                float barWidth = manaFill.rectTransform.rect.width;
                float x = Random.Range(-barWidth * 0.4f, barWidth * 0.4f);
                float y = Random.Range(5f, 25f);

                shard.transform.localPosition = new Vector2(x, y);
                shard.GetComponent<ManaShard>().Init();
            }
        }
    }

    public void RestoreMana(float amount)
    {
        if (amount <= 0f || isDead) return;
        currentMana = Mathf.Clamp(currentMana + amount, 0f, maxMana);
        manaTargetRatio = currentMana / maxMana;

        manaMainHealing = true;
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
            manaMainHealing = true;
        }
    }

    void UpdateManaUI()
    {
        if (isDead) return;

        manaTargetRatio = currentMana / maxMana;
        UpdateManaBar();
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

    void Die()
    {
        if (isDead) return;
        isDead = true;

        CurrentHealth = 0f;

        isDashInvincible = false;
        isKnockbackInvincible = false;
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

        if (gameFadePanel != null)
        {
            gameFadePanel.gameObject.SetActive(true);
            yield return StartCoroutine(FadePanel(0f, 1f));
        }

        enabled = false;
        PerformRespawn();
    }

    void RespawnImmediately()
    {
        enabled = false;
        PerformRespawn();
    }

    void PerformRespawn()
    {
        string targetScene = SceneManager.GetActiveScene().name;
        Vector3 spawnPosition = Vector3.zero;
        bool foundSave = false;
        int chosenSlot = -1;

        for (int slotIndex = 0; slotIndex < 3; slotIndex++)
        {
            SaveData saveData = SaveSystem.LoadGame(slotIndex);
            if (saveData != null && saveData.scenes != null && saveData.scenes.Count > 0)
            {
                SceneSaveData latest = saveData.scenes[saveData.scenes.Count - 1];
                targetScene = latest.sceneName;
                spawnPosition = latest.position;
                foundSave = true;
                chosenSlot = slotIndex;
                break;
            }
        }

        if (!foundSave)
        {
            GameObject defaultSpawn = GameObject.FindWithTag("Respawn");
            if (defaultSpawn != null)
            {
                spawnPosition = defaultSpawn.transform.position;
            }
            else
            {
                spawnPosition = Vector3.zero;
            }
            targetScene = SceneManager.GetActiveScene().name;
        }

        PlayerPrefs.SetString("RespawnScene", targetScene);
        PlayerPrefs.SetFloat("RespawnX", spawnPosition.x);
        PlayerPrefs.SetFloat("RespawnY", spawnPosition.y);
        PlayerPrefs.SetFloat("RespawnZ", spawnPosition.z);
        PlayerPrefs.SetInt("HasRespawnPos", foundSave ? 1 : 0);
        if (chosenSlot >= 0)
        {
            PlayerPrefs.SetInt("CurrentSlot", chosenSlot);
        }

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
    // Gọi từ animation "longden" (frame cuối)
    public void OnLongdenEquipComplete()
    {
        IsHoldingLongden = true; // ✅ DÙNG BIẾN PUBLIC
        longdenObject?.SetActive(true);
        isEquippingLongden = false;
    }

    public void OnCuocChimEquipComplete()
    {
        IsHoldingCuocChim = true; // ✅ DÙNG BIẾN PUBLIC
        cuocChimObject?.SetActive(true);
        isEquippingCuocChim = false;
    }

    public void EquipLongden()
    {
        if (IsHoldingLongden || isDead || animator == null || isEquippingLongden) return;
        isEquippingLongden = true;
        animator.Play("longden");
    }

    public void EquipCuocChim()
    {
        if (IsHoldingCuocChim || isDead || animator == null || isEquippingCuocChim) return;
        isEquippingCuocChim = true;
        animator.Play("trangbicuocchim");
    }

    public void UnequipLongden()
    {
        if (!IsHoldingLongden || animator == null) return;
        isEquippingLongden = true;
        animator.Play("unlongden");
    }
    // TRANG BỊ

    public void UnequipCuocChim()
    {
        if (!IsHoldingCuocChim || animator == null) return;
        isEquippingCuocChim = true;
        animator.Play("untrangbicuocchim");
    }

    void UseCuocChimOnTarget(Transform target)
    {
        isUsingCuocChim = true;
        isAttacking = true; // để chặn movement
        animator.Play("trangbicuocchim");
    }
    public void EnableCuocChimHitbox()
    {
        if (cuocChimHitbox != null) cuocChimHitbox.enabled = true;
    }

    public void DisableCuocChimHitbox()
    {
        if (cuocChimHitbox != null) cuocChimHitbox.enabled = false;
    }

    // Gọi từ animation "trangbicuocchim" → khi trang bị xong
    // Gọi ở frame cuối của "trangbicuocchim"
    public void OnCuocChimEquipped()
    {
        cuocChimObject.SetActive(true);
        IsHoldingCuocChim = true;
        isEquippingCuocChim = false; // ✅ mở khóa
    }

    // Gọi ở frame cuối của "untrangbicuocchim"
    public void OnCuocChimUnequipped()
    {
        cuocChimObject.SetActive(false);
        IsHoldingCuocChim = false;
        isEquippingCuocChim = false; // ✅ mở khóa
    }

    // Gọi từ animation "trangbicuocchim" → frame cuối
    public void OnLongdenUnequipComplete()
    {
        IsHoldingLongden = false;
        longdenObject?.SetActive(false);
        isEquippingLongden = false;
        justUnequippedLongden = true;
    }

    public void OnCuocChimUnequipComplete()
    {
        IsHoldingCuocChim = false;
        cuocChimObject?.SetActive(false);
        isEquippingCuocChim = false;
        justUnequippedCuocChim = true;
    }

    public void OnCuocChimSwingComplete()
    {
        isUsingCuocChim = false;
        isAttacking = false;
        // Không tự động hủy ở đây → chỉ hủy nếu đá chết
    }
    public bool ShouldDropLongdenNow()
    {
        if (longdenJustUnequipped)
        {
            longdenJustUnequipped = false;
            return true;
        }
        return false;
    }

    public bool ShouldDropCuocChimNow()
    {
        if (cuocChimJustUnequipped)
        {
            cuocChimJustUnequipped = false;
            return true;
        }
        return false;
    }


    // Gọi từ ItemSlot khi hủy trang bị (lần 1)
    public bool TryUnequipItem(string itemName)
    {
        if (itemName == "lồng đèn" && IsHoldingLongden)
        {
            UnequipLongden(); // 👈 GỌI HÀM HỦY ĐÚNG
            return true;
        }
        if (itemName == "cuốc chim" && IsHoldingCuocChim)
        {
            UnequipCuocChim(); // 👈 GỌI HÀM HỦY ĐÚNG
            return true;
        }
        return false;
    }
    // Thêm vào PlayerController
    public void CancelEquippingActions()
    {
        if (isEquippingLongden)
        {
            isEquippingLongden = false;
            // Tùy chọn: ẩn longden nếu chưa hoàn tất hủy
            // longdenObject?.SetActive(IsHoldingLongden);
        }
        if (isEquippingCuocChim)
        {
            isEquippingCuocChim = false;
            // cuocChimObject?.SetActive(IsHoldingCuocChim);
        }
    }
    void UpdateAnimation()
    {
        if (animator == null || isDead) return;
        if (isEquippingLongden && (isDashing || isAttacking || isKnockbacked))
        {
            isEquippingLongden = false;
        }
        if (isEquippingCuocChim && (isDashing || isAttacking || isKnockbacked))
        {
            isEquippingCuocChim = false;
        }
        if (isEquippingLongden) return; 
        if (isDropping) return;

        if (isEquippingCuocChim) return;
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

        if (isAttacking) return;

        if (!isGrounded)
        {
            animator.Play(rb.linearVelocity.y > 0.01f ? "Jump" : "roiiiii");
            return;
        }

        animator.Play(Mathf.Abs(horizontalInput) > 0.1f ? "Run" : "Idle");
    }
}