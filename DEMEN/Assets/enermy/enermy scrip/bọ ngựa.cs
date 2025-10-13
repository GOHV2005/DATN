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
    public int damage = 20;

    [Header("Combat Cooldown")]
    public float attackCooldown = 2f;
    private float lastAttackTime = -999f;
    
    [Header("Ẩn nấp")]
    public SpriteRenderer bodyRenderer; // Kéo SpriteRenderer của "Body" vào đây
    public float hiddenAlpha = 0.3f;    // Độ mờ khi ẩn
    public float visibleAlpha = 1f;     // Độ rõ khi hiện

    [Header("Hiệu ứng cảnh báo")]
    public GameObject warningObject; // Kéo GameObject "mắt đỏ" vào đây
    public float warningDuration = 0.4f; // Thời gian hiển thị trước khi đánh

    [Header("Tham chiếu")]
    public Transform player;

    private Rigidbody2D rb;
    private Vector2 startPos;
    private bool isAttacking = false;
    private MantisState currentState = MantisState.Idle;
    private SpriteRenderer sr;
    private Animator anim;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        startPos = transform.position;

        // Đảm bảo ban đầu không bị lật ngược
        sr.flipX = false;
        if (warningObject != null)
            warningObject.SetActive(false);
        SetBodyVisibility(false); // hoặc true nếu bạn muốn bắt đầu bằng hiện hình
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
        // Di chuyển bằng physics
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
        SetBodyVisibility(false); // 👈 ẨN THÂN

        if (distance <= visionRange)
        {
            currentState = MantisState.Detect;
            Debug.Log("👀 [Bọ ngựa] Phát hiện con mồi!");
        }
    }

    void DetectState(float distance)
    {
        FacePlayer();
        anim.Play("Dung(BoNgua)");

        if (distance <= attackRange && Time.time - lastAttackTime >= attackCooldown)
        {
            // 👇 KÍCH HOẠT CẢNH BÁO THAY VÌ TẤN CÔNG NGAY
            StartCoroutine(ShowWarningThenAttack());
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
        lastAttackTime = Time.time;
        FacePlayer();

        anim.Play("TanCong(BoNgua)");
        rb.linearVelocity = (player.position - transform.position).normalized * attackSpeed;

        Invoke(nameof(EndAttack), 0.5f);
        Debug.Log("💥 [Bọ ngựa] LAO RA TỪ BÓNG TỐI!");
    }

    void EndAttack()
    {
        rb.linearVelocity = Vector2.zero;
        isAttacking = false;
        currentState = MantisState.Reveal;
        Debug.Log("😠 [Bọ ngựa] Bị né! Hiện thân và chuẩn bị chiến đấu!");
    }

    void RevealState(float distance)
    {
        if (player == null) return;
        FacePlayer();
        anim.Play("DiBo(BoNgua)");
        SetBodyVisibility(true);
        // Kiểm tra thoát
        if (distance > visionRange)
        {
            currentState = MantisState.ReturnToIdle;
            Debug.Log("🏃 [Bọ ngựa] Mất dấu player, quay lại vị trí.");
            return;
        }

        // Tấn công trong Reveal
        if (distance <= revealAttackRange && !isAttacking && Time.time - lastAttackTime >= attackCooldown)
        {
            Debug.Log("🩸 [Bọ ngựa] Vồ tấn công trực diện!");
            PerformRevealAttack();
        }
    }

    void PerformRevealAttack()
    {
        if (player == null) return;

        isAttacking = true;
        lastAttackTime = Time.time;
        FacePlayer();

        // 👇 DÙNG ĐÚNG TÊN ANIMATION MÀ "TẤN CÔNG CHỚP NHOÁNG" DÙNG
        anim.Play("TanCong(BoNgua)"); // hoặc tên khác nếu bạn đặt khác

        rb.linearVelocity = (player.position - transform.position).normalized * (attackSpeed * 1.2f);
        Invoke(nameof(EndRevealAttack), 0.4f);
        Debug.Log("🩸 [Bọ ngựa] Vồ tấn công trực diện!");
    }

    void EndRevealAttack()
    {
        rb.linearVelocity = Vector2.zero;
        isAttacking = false;
        Debug.Log("🔁 [Bọ ngựa] Kết thúc combo, chờ cơ hội tấn công tiếp.");
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
            SetBodyVisibility(false); // 👈 ẨN LẠI KHI VỀ VỊ TRÍ
            currentState = MantisState.Idle;
            Debug.Log("🌿 [Bọ ngựa] Đã trở về vị trí ẩn nấp.");
        }
    }

    // ================== FLIP SPRITE (AN TOÀN VỚI ANIMATION) ==================

    void FacePlayer()
    {
        if (player == null) return;
        sr.flipX = !(player.position.x < transform.position.x);
    }

    void FaceStartPosition()
    {
        sr.flipX = !(startPos.x < transform.position.x);
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
        // 👉 GIỮ THÂN Ở TRẠNG THÁI ẨN (mờ) trong suốt thời gian cảnh báo
        SetBodyVisibility(false); // Đảm bảo thân vẫn mờ

        // BẬT MẮT ĐỎ – LUÔN RÕ
        if (warningObject != null)
        {
            warningObject.SetActive(true);
            var eyeRenderer = warningObject.GetComponent<SpriteRenderer>();
            if (eyeRenderer != null)
            {
                Color eyeColor = eyeRenderer.color;
                eyeColor.a = 1f; // Sáng rực
                eyeRenderer.color = eyeColor;
            }
        }

        Debug.Log("⚠️ [Bọ ngựa] CẢNH BÁO – THÂN VẪN ẨN!");
        yield return new WaitForSeconds(warningDuration);

        // 👇 SAU CẢNH BÁO: ẨN MẮT ĐỎ + HIỆN RÕ THÂN NGAY TRƯỚC KHI TẤN CÔNG
        if (warningObject != null)
            warningObject.SetActive(false);

        SetBodyVisibility(true); // HIỆN RÕ THÂN

        // TIẾN HÀNH TẤN CÔNG
        currentState = MantisState.Attack;
        PerformAttack();
    }
}