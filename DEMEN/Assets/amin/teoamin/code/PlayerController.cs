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

    [Header("Footstep Sounds")]
    public AudioClip[] footstepClips; // Kéo 2 âm thanh vào đây
    public float footstepInterval = 0.35f;
    public float footstepVolume = 0.6f;

    private float lastFootstepTime = 0f;
    private int nextFootstepIndex = 0;
    private AudioSource audioSource;

    [Header("Ground Check")]
    public Transform groundCheck;          // 👈 Gán điểm kiểm tra dưới chân
    public float groundCheckRadius = 0.2f; // Bán kính kiểm tra
    public LayerMask groundLayer;          // 👈 Hoặc dùng Tag (xem dưới)
    public string groundTag = "Ground";    // 👈 DÙNG TAG NHƯ BẠN MUỐN

    [Header("Wall Cling")]
    public Transform wallCheck;
    public float wallCheckRadius = 0.2f;
    public float wallDetachDelay = 0.12f;

    // 👇 BIẾN MỚI
    public float wallClingManaDrainPerSecond = 15f; // mana tiêu mỗi giây khi bám

    private bool isWallClinging = false;
    private float wallClingTimer = 0f;
    private float wallDetachTimer = 0f;
    private float wallClingStartX = 0f;

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
    public bool IsHoldingLongden { get; set; } = false;
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

    public bool IsHoldingCuocChim { get; set; } = false;

    [Header("Kiếm")]
    public GameObject kiemObject;
    public PolygonCollider2D kiemHitbox;
    private bool isEquippingKiem = false;
    public bool IsHoldingKiem { get; set; } = false;
    public bool kiemJustUnequipped = false;
    public bool justUnequippedKiem = false;

    public void MarkKiemAsJustUnequipped() => kiemJustUnequipped = true;

    [Header("Jump Float")]
    public bool useJumpFloat = true;
    public float floatGravityScale = 0.3f;

    [Header("UI - Health (Heart System)")]
    public Image[] heartImages;
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;
    public float healthPerHeart = 20f;
    public System.Action onTakeDamage; // 👈 THÊM DÒNG NÀY

    [Header("Audio Clips")]
    public AudioClip jumpSound;
    public AudioClip equipKiemSound;
    public AudioClip attackNormalSound;
    public AudioClip attackKiemSound;

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
    private bool isDropConfirmed = false; // 👈 MỚI

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
  
    private float manaTargetRatio;
    private bool manaMainHealing = false;

    public float CurrentHealth
    {
        get => currentHealth;
        set
        {
            currentHealth = Mathf.Clamp(value, 0f, maxHealth);
            UpdateHeartUI();
        }
    }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        rb = GetComponent<Rigidbody2D>();
        defaultGravityScale = rb.gravityScale; // ✅ ĐÚNG TÊN BIẾN
        spriteRenderers = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>(true));

        // 👇 KHỞI TẠO AUDIO SOURCE
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound
        }
    }

    void Start()
    {
        CurrentHealth = maxHealth;
        currentMana = Mathf.Clamp(currentMana, 0f, maxMana);
        manaTargetRatio = currentMana / maxMana;

        if (manaFill != null) manaFill.fillAmount = manaTargetRatio;

        int currentSlot = PlayerPrefs.GetInt("CurrentSlot", 0);
        SaveData saveData = SaveSystem.LoadGame(currentSlot);

        // 👇 KHÔI PHỤC INVENTORY VÀ TRẠNG THÁI TRANG BỊ
        if (saveData != null)
        {
            saveData.RestoreInventory(); // 👈 DÒNG QUAN TRỌNG NHẤT
        }

        // 👇 SAU ĐÓ MỚI LOAD VỊ TRÍ
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
        CheckGroundAndWall();

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

            // 👇 Ngăn dash khi đang bám tường
            if (Input.GetKeyDown(KeyCode.Q) && !isWallClinging)
                dashRequested = true;

            if (Input.GetMouseButtonDown(0)) attackRequested = true;
        }
        else
        {
            horizontalInput = 0f;
        }
        if (Mathf.Abs(horizontalInput) > 0.1f && isGrounded && !isDashing && !isAttacking && !isKnockbacked && !isDead)
        {
            if (Time.time - lastFootstepTime >= footstepInterval)
            {
                if (footstepClips != null && footstepClips.Length > 0)
                {
                    // Luân phiên: 0 → 1 → 0 → 1...
                    audioSource.PlayOneShot(footstepClips[nextFootstepIndex], footstepVolume);
                    nextFootstepIndex = (nextFootstepIndex + 1) % footstepClips.Length;
                    lastFootstepTime = Time.time;
                }
            }
        }
    }
    void CheckGroundAndWall()
    {
        // --- Ground check (ưu tiên cao nhất) ---
        bool wasGrounded = isGrounded;
        isGrounded = false;
        Collider2D[] groundColliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius);
        foreach (Collider2D col in groundColliders)
        {
            if (col.CompareTag(groundTag))
            {
                isGrounded = true;
                break;
            }
        }

        // Reset jump khi chạm đất
        if (!wasGrounded && isGrounded)
        {
            jumpCount = 0;
            isWallClinging = false;
            wallClingTimer = 0f;
            wallDetachTimer = 0f;
        }

        // --- Wall check (chỉ hoạt động nếu KHÔNG grounded) ---
        if (!isGrounded && wallCheck != null)
        {
            bool wallDetected = false;
            Collider2D[] wallColliders = Physics2D.OverlapCircleAll(wallCheck.position, wallCheckRadius);
            foreach (Collider2D col in wallColliders)
            {
                if (col.CompareTag(groundTag))
                {
                    wallDetected = true;
                    break;
                }
            }
            if (wallDetected)
            {
                // ✅ CHỈ CHO BÁM NẾU MANA ĐỦ (≥ 20%)
                if (!isWallClinging && currentMana >= maxMana * 0.2f)
                {
                    isWallClinging = true;
                    wallClingTimer = 0f;
                    wallClingStartX = transform.position.x;
                }
                wallDetachTimer = 0f;
            }
            else
            {
                if (isWallClinging)
                {
                    wallDetachTimer += Time.deltaTime;
                    if (wallDetachTimer >= wallDetachDelay)
                    {
                        isWallClinging = false;
                        wallClingTimer = 0f;
                    }
                }
            }
        }
        else
        {
            // Không grounded nhưng không có wallCheck → tắt
            if (isWallClinging)
            {
                isWallClinging = false;
                wallClingTimer = 0f;
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

        // 👇 XỬ LÝ NHẢY (wall jump + ground jump) — CHỈ CÓ 1 LẦN
        if (jumpRequested)
        {
            if (isWallClinging)
            {
                // Wall Jump: nhảy ra khỏi tường
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(new Vector2((facingRight ? -1f : 1f) * jumpForce * 0.8f, jumpForce), ForceMode2D.Impulse);
                jumpCount = 1; // đã dùng 1 lần jump
                isWallClinging = false;
            }
            else if (jumpCount < maxJumpCount)
            {
                HandleJump();
            }
            jumpRequested = false;
        }

        // 👇 XỬ LÝ DASH — NGĂN KHI BÁM TƯỜNG
        if (dashRequested && !isWallClinging)
        {
            HandleDash();
            dashRequested = false;
        }

        // 👇 DI CHUYỂN — NGĂN KHI BÁM TƯỜNG
        if (!isDashing && !isKnockbacked && !isWallClinging)
        {
            HandleMovement();
        }

        // 👇 XỬ LÝ BÁM TƯỜNG — ĐỨNG YÊN HOÀN TOÀN
        if (isWallClinging)
        {
            // Giữ nguyên vị trí X khi bắt đầu bám
            transform.position = new Vector3(wallClingStartX, transform.position.y, transform.position.z);
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;

            // Tiêu hao mana
            currentMana -= wallClingManaDrainPerSecond * Time.fixedDeltaTime;
            if (currentMana < 0f) currentMana = 0f;

            // Cập nhật UI mana real-time
            if (manaFill != null)
            {
                manaFill.fillAmount = currentMana / maxMana;
            }

            // Hết mana → ngưng bám
            if (currentMana <= 0f)
            {
                isWallClinging = false;
                wallClingTimer = 0f;
            }
        }
        else
        {
            // 👇 KHÔI PHỤC GRAVITY — KẾT HỢP JUMP FLOAT
            if (useJumpFloat && !isGrounded && rb.linearVelocity.y < 0f)
            {
                rb.gravityScale = Input.GetKey(KeyCode.Space) ? floatGravityScale : defaultGravityScale;
            }
            else
            {
                rb.gravityScale = defaultGravityScale;
            }
        }

        // 👇 KHÔNG CÓ KHỐI ĐẶT GRAVITY NÀO Ở DƯỚI NỮA → TRÁNH GHI ĐÈ
    }

    public void DropItem(string name, int qty, Sprite sprite, string desc, System.Action onComplete = null)
    {
        CancelEquippingActions();
        if (animator == null || isDead) return;

        CancelDrop();

        isDropping = true;
        isDropConfirmed = false;
        dropOnComplete = onComplete;

        animator.Play("dropItem");
        StartCoroutine(DelayedDropSpawn(name));
    }

    private IEnumerator DelayedDropSpawn(string itemName)
    {
        yield return new WaitForSeconds(1.15f);

        if (!isDropping) yield break;
        isDropConfirmed = true;
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
        if(isDropConfirmed)return;
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
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpCount++;
            isGrounded = false;

            // Phát âm thanh nhảy
            if (jumpSound != null && audioSource != null)
                audioSource.PlayOneShot(jumpSound, 0.8f);
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

        string attackAnim = "Attack";
        AudioClip soundToPlay = attackNormalSound; // mặc định

        if (IsHoldingKiem)
        {
            attackAnim = "Chem";
            soundToPlay = attackKiemSound;
        }
        else if (IsHoldingCuocChim)
        {
            attackAnim = "sudungcuocchim";
            soundToPlay = attackNormalSound; // hoặc âm thanh riêng nếu muốn
        }

        animator.Play(attackAnim);

        attackedEnemies.Clear();

        Collider2D hitbox = null;
        if (IsHoldingKiem) hitbox = kiemHitbox;
        else if (IsHoldingCuocChim) hitbox = cuocChimHitbox;
        else hitbox = attackHitbox;

        if (hitbox != null)
        {
            hitbox.enabled = true;
            StartCoroutine(DisableHitboxAfterDelay(GetAnimationLength(attackAnim), hitbox));
        }

        attackCooldownTimer = attackCooldown;

        // Phát âm thanh tấn công
        if (soundToPlay != null && audioSource != null)
            audioSource.PlayOneShot(soundToPlay, 0.8f);
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

        // 👇 XỬ LÝ VŨ KHÍ
        if (other.CompareTag("rock") || other.CompareTag("Enemy")|| other.CompareTag("co"))
        {
            // KIẾM
            if (kiemHitbox != null && kiemHitbox.enabled)
            {
                if (!other.CompareTag("rock")) // 👈 KHÔNG ĐÁNH ROCK
                {
                    Health health = other.GetComponent<Health>();
                    if (health != null)
                    {
                        health.TakeDamage(damageOnTouch);
                    }
                }
                return;
            }

            // CUỐC CHIM
            if (cuocChimHitbox != null && cuocChimHitbox.enabled)
            {
                Health health = other.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(cuocChimDamage);
                    if (other.CompareTag("rock"))
                    {
                        health.onDeath += OnRockDestroyed;
                    }
                }
                return;
            }

            // TẤN CÔNG THƯỜNG
            if (attackHitbox != null && attackHitbox.enabled && !attackedEnemies.Contains(other))
            {
                if (!other.CompareTag("rock")) // 👈 KHÔNG ĐÁNH ROCK BẰNG TAY THƯỜNG
                {
                    Health health = other.GetComponent<Health>();
                    if (health != null)
                    {
                        attackedEnemies.Add(other);
                        health.TakeDamage(damageOnTouch);
                    }
                }
                return;
            }
        }

        // 👇 DAMAGE TỪ ENEMY
        if (other.CompareTag("Enemy") && !isDashInvincible && !isKnockbackInvincible /*&& !isAttacking*/)
        {
            if (other.GetComponent<BossMantisAI> () != null)
                return;
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
        // Vòng Ground Check — ưu tiên cao nhất
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // 🟠 Vòng Wall Check — chỉ 1 lần
        if (wallCheck != null)
        {
            Color c = Application.isPlaying
                ? (isWallClinging ? Color.magenta : Color.grey)
                : new Color(1f, 0.5f, 0f, 0.7f);
            Gizmos.color = c;
            Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
        }
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
        if (isDashing || isDead || isWallClinging) return;

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
        int currentSlot = PlayerPrefs.GetInt("CurrentSlot", 0);
        SaveData saveData = SaveSystem.LoadGame(currentSlot);

        string targetScene = SceneManager.GetActiveScene().name;
        Vector3 spawnPosition = Vector3.zero;
        bool foundSave = false;

        // 👇 CHỈ DÙNG SAVE CỦA CURRENT SLOT — KHÔNG DUYỆT QUA CÁC SLOT KHÁC
        if (saveData != null && saveData.scenes != null && saveData.scenes.Count > 0)
        {
            SceneSaveData latest = saveData.scenes[saveData.scenes.Count - 1];
            targetScene = latest.sceneName;
            spawnPosition = latest.position;
            foundSave = true;
        }

        // Nếu không có save trong current slot → dùng vị trí mặc định trong scene hiện tại
        if (!foundSave)
        {
            GameObject defaultSpawn = GameObject.FindWithTag("Respawn");
            if (defaultSpawn != null)
            {
                spawnPosition = defaultSpawn.transform.position;
                targetScene = SceneManager.GetActiveScene().name; // luôn ở scene hiện tại
            }
            else
            {
                spawnPosition = Vector3.zero;
                targetScene = SceneManager.GetActiveScene().name;
            }
        }

        // Lưu thông tin respawn (dù có save hay không)
        PlayerPrefs.SetString("RespawnScene", targetScene);
        PlayerPrefs.SetFloat("RespawnX", spawnPosition.x);
        PlayerPrefs.SetFloat("RespawnY", spawnPosition.y);
        PlayerPrefs.SetFloat("RespawnZ", spawnPosition.z);
        PlayerPrefs.SetInt("HasRespawnPos", foundSave ? 1 : 0);
        // 👇 GIỮ NGUYÊN CURRENT SLOT — KHÔNG GÁN LẠI
        // PlayerPrefs.SetInt("CurrentSlot", currentSlot); // không cần, vì đã là current rồi

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
        if (IsHoldingKiem) UnequipKiem();
        OnKiemUnequipComplete();
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

    public void OnCuocChimEquipped()
    {
        cuocChimObject.SetActive(true);
        IsHoldingCuocChim = true;
        isEquippingCuocChim = false; 
    }

    public void OnCuocChimUnequipped()
    {
        cuocChimObject.SetActive(false);
        IsHoldingCuocChim = false;
        isEquippingCuocChim = false;
    }

 
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
            UnequipLongden(); 
            return true;
        }
        if (itemName == "cuốc chim" && IsHoldingCuocChim)
        {
            UnequipCuocChim(); 
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

        }
        if (isEquippingCuocChim)
        {
            isEquippingCuocChim = false;

        }
        if (isEquippingKiem) isEquippingKiem = false;
    }
    public void EquipKiem()
    {
        if (IsHoldingKiem || isDead || animator == null || isEquippingKiem) return;

        if (IsHoldingCuocChim) UnequipCuocChim();
        OnCuocChimUnequipped();
        isEquippingKiem = true;
        animator.Play("trangbikiem");
    }

    public void UnequipKiem()
    {
        if (!IsHoldingKiem || animator == null) return;
        isEquippingKiem = true;
        animator.Play("untrangbikiem");
    }

    // Animation Event: frame cuối của "trangbikiem"
    public void OnKiemEquipComplete()
    {
        IsHoldingKiem = true;
        kiemObject?.SetActive(true);
        isEquippingKiem = false;

        // Phát âm thanh trang bị kiếm
        if (equipKiemSound != null && audioSource != null)
            audioSource.PlayOneShot(equipKiemSound, 0.8f);
    }


    // Animation Event: frame cuối của "untrangbikiem"
    public void OnKiemUnequipComplete()
    {
        IsHoldingKiem = false;
        kiemObject?.SetActive(false);
        isEquippingKiem = false;
        justUnequippedKiem = true;
    }
    void UpdateAnimation()
    {
        if (animator == null || isDead) return;

        // Hủy các hành động đang diễn ra nếu có va chạm mâu thuẫn
        if (isEquippingLongden && (isDashing || isAttacking || isKnockbacked))
            isEquippingLongden = false;
        if (isEquippingCuocChim && (isDashing || isAttacking || isKnockbacked))
            isEquippingCuocChim = false;

        if (isEquippingLongden || isEquippingCuocChim || isEquippingKiem || isDropping)
            return;

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
            return;

        // ✅ ƯU TIÊN: nếu grounded → không bám tường
        if (isGrounded)
        {
            animator.Play(Mathf.Abs(horizontalInput) > 0.1f ? "Run" : "Idle");
            return;
        }

        // ✅ BÁM TƯỜNG
        if (isWallClinging)
        {
            animator.Play("bamtuong");
            return;
        }

        // Nhảy/rơi
        animator.Play(rb.linearVelocity.y > 0.01f ? "Jump" : "roiiiii");
    }
}