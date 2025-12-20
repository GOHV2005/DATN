using System.Collections;
using UnityEngine;

public class MantisAI : MonoBehaviour
{
    public enum MantisState { Idle, Detect, Attack, Reveal, ReturnToIdle }

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
    private float currentAttackDamage = 0f;
    private bool isAttackActive = false;

    // 👇 BIẾN MỚI: GHI NHỚ ĐÃ DÙNG SKILL 1
    private bool usedStealthAttack = false;

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
            Debug.Log("👀 [Bọ ngựa] Phát hiện con mồi!");
        }
    }

    void DetectState(float distance)
    {
        FacePlayer();
        anim.Play("Dung(BoNgua)");

        if (distance <= attackRange && Time.time - lastAttackTime >= attackCooldown)
        {
            if (!usedStealthAttack)
            {
                Debug.Log("🎯 [Bọ ngựa] Dùng Skill 1 (Tàng hình vồ bất ngờ)");
                StartCoroutine(ShowWarningThenAttack());
            }
            else
            {
                PerformRevealAttack();
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
        currentState = MantisState.Reveal;
        Debug.Log("😠 [Bọ ngựa] Bị né! Hiện thân và chuẩn bị chiến đấu!");
    }

    void RevealState(float distance)
    {
        if (player == null) return;
        FacePlayer();

        if (!isAttacking)
            anim.Play("DiBo(BoNgua)");

        SetBodyVisibility(true);

        if (distance > visionRange)
        {
            currentState = MantisState.ReturnToIdle;
            Debug.Log("🏃 [Bọ ngựa] Mất dấu player, quay lại vị trí.");
            return;
        }

        if (distance <= revealAttackRange && !isAttacking && Time.time - lastAttackTime >= attackCooldown)
        {
            PerformRevealAttack();
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

    void ReturnToIdleState()
    {
        FaceStartPosition();
        anim.Play("DiBo(BoNgua)");
        SetBodyVisibility(true);

        // Chỉ di chuyển theo trục X
        float dirX = startPos.x - transform.position.x;
        if (Mathf.Abs(dirX) > 0.05f) // khoảng nhỏ để tránh rung lắc
        {
            float moveStep = Mathf.Sign(dirX) * returnSpeed * Time.deltaTime;
            if (Mathf.Abs(moveStep) > Mathf.Abs(dirX)) moveStep = dirX; // không vượt quá điểm đích
            transform.position += new Vector3(moveStep, 0, 0);
        }
        else
        {
            // Về đúng X, dừng hẳn
            rb.linearVelocity = Vector2.zero;
            anim.Play("Dung(BoNgua)");
            SetBodyVisibility(false);
            currentState = MantisState.Idle;
            Debug.Log("🌿 [Bọ ngựa] Đã trở về vị trí ẩn nấp.");
        }
    }


    void FacePlayer()
    {
        if (player == null) return;
        sr.flipX = !(player.position.x < transform.position.x);
    }

    void FaceStartPosition()
    {
        sr.flipX = !(startPos.x < transform.position.x);
    }

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
