using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MantisAI : MonoBehaviour
{
    public enum MantisState { Idle, Detect, Attack, Reveal, ReturnToIdle, StrongAttack }

    [Header("Thiết lập cơ bản")]
    public float visionRange = 6f;
    public float attackRange = 2f;
    public float revealAttackRange = 1.5f;
    public float attackSpeed = 8f;
    public float moveSpeed = 2f;
    public float returnSpeed = 3f;

    [Header("Skill 1: Tàng Hình Vồ Bất Ngờ")]
    public int stealthAttackDamage = 100;
    public float stealthWarningDuration = 0.4f;

    [Header("Skill 2: Lao Đánh Trực Diện")]
    public int directAttackDamage = 60;

    [Header("Skill 3: Tấn Công Mạnh (Sóng bay)")]
    public int strongAttackDamage = 80;
    public float strongAttackRange = 3f;
    public float strongAttackCooldown = 5f;
    private float lastStrongAttackTime = -999f;
    public GameObject shockwaveProjectilePrefab;

    [Header("Combat Cooldown")]
    public float attackCooldown = 2f;
    private float lastAttackTime = -999f;

    [Header("Ẩn nấp")]
    public SpriteRenderer bodyRenderer;
    public float hiddenAlpha = 0.3f;
    public float visibleAlpha = 1f;

    [Header("Hiệu ứng cảnh báo")]
    public GameObject warningObject;
    public float warningDuration = 0.4f;

    [Header("Hiệu ứng damage")]
    public float flashDuration = 0.15f;
    public float flashFrequency = 10f;
    private SpriteRenderer[] spriteRenderers;

    [Header("Tham chiếu")]
    public Transform player;

    private Rigidbody2D rb;
    private Vector2 startPos;
    private bool isAttacking = false;
    private MantisState currentState = MantisState.Idle;
    private SpriteRenderer sr;
    private Animator anim;
    private MantisState lastAttackState = MantisState.Idle;
    private float currentAttackDamage = 0f;
    private bool isAttackActive = false;

    // 👇 BIẾN MỚI: GHI NHỚ ĐÃ DÙNG SKILL 1
    private bool usedStealthAttack = false;
    private bool canUseStrongAttack = false; // 👈 CHỈ MỞ KHÓA SAU KHI DÙNG SKILL 1

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        startPos = transform.position;

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        sr.flipX = false;
        if (warningObject != null)
            warningObject.SetActive(false);
        SetBodyVisibility(false);
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 👇 KIỂM TRA LIÊN TỤC ĐIỀU KIỆN DÙNG SKILL 3 (CHỈ SAU KHI DÙNG SKILL 1)
        if (canUseStrongAttack && !isAttacking && !isAttackActive && currentState != MantisState.StrongAttack)
        {
            if (distanceToPlayer > strongAttackRange && distanceToPlayer < visionRange && Time.time - lastStrongAttackTime >= strongAttackCooldown)
            {
                Debug.Log("🎯 [Bọ ngựa] Dùng Skill 3 (Tấn công mạnh)!");
                StartCoroutine(PerformStrongAttack());
            }
        }

        switch (currentState)
        {
            case MantisState.Idle:
                IdleState(distanceToPlayer);
                break;
            case MantisState.Detect:
                DetectState(distanceToPlayer);
                break;
            case MantisState.Attack:
                // AttackState xử lý trong FixedUpdate hoặc qua flag
                break;
            case MantisState.Reveal:
                RevealState(distanceToPlayer);
                break;
            case MantisState.ReturnToIdle:
                ReturnToIdleState();
                break;
            case MantisState.StrongAttack:
                // Không làm gì, animation sẽ xử lý
                break;
        }
    }

    void FixedUpdate()
    {
        if (currentState == MantisState.Reveal && !isAttacking)
        {
            if (player != null)
            {
                Vector2 moveDir = (player.position - transform.position).normalized;
                rb.linearVelocity = new Vector2(moveDir.x * moveSpeed, rb.linearVelocity.y);
            }
        }
        else if (currentState == MantisState.ReturnToIdle)
        {
            Vector2 dir = (startPos - (Vector2)transform.position).normalized;
            rb.linearVelocity = new Vector2(dir.x * returnSpeed, rb.linearVelocity.y);
        }
        else if (currentState == MantisState.Idle || currentState == MantisState.Detect)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // ================== STATE HANDLERS ==================
    void SetBodyVisibility(bool isVisible)
    {
        if (bodyRenderer == null) return;

        Color color = bodyRenderer.color;
        color.a = isVisible ? visibleAlpha : hiddenAlpha;
        bodyRenderer.color = color;
    }

    void IdleState(float distance)
    {
        anim.Play("Dung(BoNgua)");
        SetBodyVisibility(false);

        if (distance <= visionRange)
        {
            currentState = MantisState.Detect;
            usedStealthAttack = false;
            canUseStrongAttack = false; // 👈 RESET KHI BẮT ĐẦU PHÁT HIỆN
            Debug.Log("👀 [Bọ ngựa] Phát hiện con mồi!");
        }
    }

    void DetectState(float distance)
    {
        FacePlayer();
        anim.Play("Dung(BoNgua)");

        if (distance <= attackRange && Time.time - lastAttackTime >= attackCooldown)
        {
            // 👇 ƯU TIÊN DÙNG SKILL 1 KHI BẮT ĐẦU PHÁT HIỆN
            if (!usedStealthAttack)
            {
                Debug.Log("🎯 [Bọ ngựa] Dùng Skill 1 (Tàng hình vồ bất ngờ)");
                StartCoroutine(ShowWarningThenAttack());
            }
            else
            {
                // 👇 SAU KHI DÙNG SKILL 1 → MỞ KHÓA SKILL 3
                if (canUseStrongAttack && distance > strongAttackRange && distance < visionRange && Time.time - lastStrongAttackTime >= strongAttackCooldown)
                {
                    Debug.Log("🎯 [Bọ ngựa] Dùng Skill 3 (Tấn công mạnh)!");
                    StartCoroutine(PerformStrongAttack());
                }
                else
                {
                    PerformRevealAttack();
                }
            }
        }
        else if (distance > visionRange)
        {
            currentState = MantisState.ReturnToIdle;
            Debug.Log("❌ [Bọ ngựa] Mất dấu con mồi, quay lại vị trí.");
        }
    }

    void PerformAttack()
    {
        if (player == null) return;

        isAttacking = true;
        currentAttackDamage = stealthAttackDamage;
        isAttackActive = true;
        lastAttackTime = Time.time;
        FacePlayer();

        anim.Play("TanCong(BoNgua)");
        rb.linearVelocity = (player.position - transform.position).normalized * attackSpeed;

        Invoke(nameof(EndAttack), 0.5f);
        Debug.Log("💥 [Bọ ngựa] LAO RA TỪ BÓNG TỐI! (Skill 1)");
    }

    void EndAttack()
    {
        rb.linearVelocity = Vector2.zero;
        isAttacking = false;
        isAttackActive = false;
        usedStealthAttack = true;
        canUseStrongAttack = true; // 👈 MỞ KHÓA SKILL 3 SAU KHI DÙNG SKILL 1
        currentState = MantisState.Reveal;
        Debug.Log("😠 [Bọ ngựa] Bị né! Hiện thân và chuẩn bị chiến đấu!");
    }

    void RevealState(float distance)
    {
        if (player == null) return;
        FacePlayer();

        if (!isAttacking)
        {
            anim.Play("DiBo(BoNgua)");
        }

        SetBodyVisibility(true);

        if (distance > visionRange)
        {
            currentState = MantisState.ReturnToIdle;
            Debug.Log("🏃 [Bọ ngựa] Mất dấu player, quay lại vị trí.");
            return;
        }

        if (distance <= revealAttackRange && !isAttacking && Time.time - lastAttackTime >= attackCooldown)
        {
            // 👇 CHỈ DÙNG SKILL 2 & 3 SAU KHI DÙNG SKILL 1
            if (canUseStrongAttack && distance > strongAttackRange && distance < visionRange && Time.time - lastStrongAttackTime >= strongAttackCooldown)
            {
                Debug.Log("🎯 [Bọ ngựa] Dùng Skill 3 (Tấn công mạnh)!");
                StartCoroutine(PerformStrongAttack());
            }
            else
            {
                PerformRevealAttack();
            }
        }
    }

    void PerformRevealAttack()
    {
        if (player == null) return;

        isAttacking = true;
        currentAttackDamage = directAttackDamage;
        isAttackActive = true;
        lastAttackTime = Time.time;
        FacePlayer();

        anim.Play("TanCong(BoNgua)", 0, 0f);

        rb.linearVelocity = (player.position - transform.position).normalized * (attackSpeed * 1.2f);
        Invoke(nameof(EndRevealAttack), 0.4f);
        Debug.Log("🩸 [Bọ ngựa] Vồ tấn công trực diện! (Skill 2)");
    }

    void EndRevealAttack()
    {
        rb.linearVelocity = Vector2.zero;
        isAttacking = false;
        isAttackActive = false;
        Debug.Log("🔁 [Bọ ngựa] Kết thúc combo, chờ cơ hội tấn công tiếp.");
    }

    // 👇 HÀM MỚI: THỰC HIỆN SKILL MẠNH (BẮN SÓNG VỀ PHÍA PLAYER)
    IEnumerator PerformStrongAttack()
    {
        if (player == null) yield break;

        currentState = MantisState.StrongAttack;
        isAttacking = true;
        FacePlayer();
        anim.Play("TanCongManh(Bongua)");

        yield return new WaitForSeconds(0.8f);

        SpawnShockwaveProjectile();
        lastStrongAttackTime = Time.time;

        isAttacking = false;
        currentState = MantisState.Reveal;
        Debug.Log("💥 [Bọ ngựa] BẮN SÓNG NĂNG LƯỢNG VỀ PHÍA PLAYER!");
    }

    // 👇 BẮN 1 VIÊN SÓNG VỀ PHÍA PLAYER
    // 👇 BẮN 1 VIÊN SÓNG THEO HƯỚNG MANTIS NHÌN
    void SpawnShockwaveProjectile()
    {
        if (shockwaveProjectilePrefab == null)
        {
            Debug.LogWarning("Chưa có prefab sóng năng lượng!");
            return;
        }

        GameObject wave = Instantiate(shockwaveProjectilePrefab, transform.position, Quaternion.identity);
        ShockwaveProjectile shockwave = wave.GetComponent<ShockwaveProjectile>();
        if (shockwave != null)
        {
            // 👇 GỬI HƯỚNG THEO TRỤC X: 1 nếu nhìn phải, -1 nếu nhìn trái
            float dirX = sr.flipX ? 1f : -1f; // sr là SpriteRenderer của Mantis
            shockwave.Initialize(dirX, strongAttackDamage);
        }
    }

    void ReturnToIdleState()
    {
        FaceStartPosition();
        anim.Play("DiBo(BoNgua)");
        SetBodyVisibility(true);
        if (Vector2.Distance(transform.position, startPos) < 0.1f)
        {
            rb.linearVelocity = Vector2.zero;
            anim.Play("Dung(BoNgua)");
            SetBodyVisibility(false);
            currentState = MantisState.Idle;
            Debug.Log("🌿 [Bọ ngựa] Đã trở về vị trí ẩn nấp.");
        }
    }

    // ================== FLIP SPRITE ==================
    void FacePlayer()
    {
        if (player == null) return;
        sr.flipX = !(player.position.x < transform.position.x);
    }

    void FaceStartPosition()
    {
        sr.flipX = !(startPos.x < transform.position.x);
    }

    // ================== DAMAGE & FLASH ==================
    public void TakeDamage(float amount)
    {
        Debug.Log($"[Enemy] Nhận {amount} sát thương!");
        // 👇 BỎ DÒNG NÀY: StartCoroutine(FlashWhite());
    }

    // ================== TRIGGER DAMAGE ==================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isAttackActive)
        {
            PlayerController playerScript = other.GetComponent<PlayerController>();
            if (playerScript != null)
            {
                playerScript.TakeDamageFromEnemy(currentAttackDamage, transform.position);
                Debug.Log($"[Enemy] Gây {currentAttackDamage} sát thương!");
            }
        }
    }

    // ================== DEBUG VIZ ==================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        DrawSquare(transform.position, visionRange);
        Gizmos.color = Color.red;
        DrawSquare(transform.position, attackRange);
        Gizmos.color = Color.magenta;
        DrawSquare(transform.position, revealAttackRange);
        Gizmos.color = Color.blue;
        DrawSquare(transform.position, strongAttackRange);
    }

    void DrawSquare(Vector2 center, float size)
    {
        float half = size / 2f;
        Vector3 tl = center + new Vector2(-half, half);
        Vector3 tr = center + new Vector2(half, half);
        Vector3 br = center + new Vector2(half, -half);
        Vector3 bl = center + new Vector2(-half, -half);

        Gizmos.DrawLine(tl, tr);
        Gizmos.DrawLine(tr, br);
        Gizmos.DrawLine(br, bl);
        Gizmos.DrawLine(bl, tl);
    }

    IEnumerator ShowWarningThenAttack()
    {
        SetBodyVisibility(false);

        if (warningObject != null)
        {
            warningObject.SetActive(true);
            var eyeRenderer = warningObject.GetComponent<SpriteRenderer>();
            if (eyeRenderer != null)
            {
                Color eyeColor = eyeRenderer.color;
                eyeColor.a = 1f;
                eyeRenderer.color = eyeColor;
            }
        }

        Debug.Log("⚠️ [Bọ ngựa] CẢNH BÁO – THÂN VẪN ẨN!");
        yield return new WaitForSeconds(stealthWarningDuration);

        if (warningObject != null)
            warningObject.SetActive(false);

        SetBodyVisibility(true);

        currentState = MantisState.Attack;
        PerformAttack();
    }
}