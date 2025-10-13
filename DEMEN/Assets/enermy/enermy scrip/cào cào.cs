using UnityEngine;
using System.Collections;

public class EnemyGrasshopper : MonoBehaviour
{
    // === ĐIỂM TUẦN TRA ===
    public Transform pointA;
    public Transform pointB;
    public float patrolDistance = 5f;
    public float viewAngle = 120f; // Góc nhìn (60° = 30° sang mỗi bên)
    public LayerMask obstacleLayer; // Layer của nền, tường, vật cản

    // === NHẢY TUẦN TRA ===
    public float stepJumpDistance = 1.5f;   // Mỗi lần nhảy bao xa (ngang)
    public float stepJumpHeight = 3f;       // Độ cao mỗi lần nhảy
    public float idleTime = 0.8f;           // Nghỉ sau khi tiếp đất

    // === TẤN CÔNG PLAYER ===
    public float detectionRange = 6f;
    public float groundTolerance = 0.5f;
    public float chargeTime = 2f;
    public float attackJumpTime = 0.8f;

    // === THAM CHIẾU ===
    public Transform player;

    // === THÀNH PHẦN ===
    private Rigidbody2D rb;
    private Vector3 originalLocalScale;
    private bool isGrounded = false;
    private bool movingRight = true;

    private enum State { Patrolling, Charging, Jumping, Scanning }
    private State currentState = State.Patrolling;

    private Coroutine patrolCoroutine;
    private Coroutine chargeCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalLocalScale = transform.localScale;

        if (pointA == null || pointB == null)
        {
            pointA = transform;
            var tempB = new GameObject("Grasshopper_PointB");
            tempB.transform.position = transform.position + Vector3.right * patrolDistance;
            pointB = tempB.transform;
        }

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        StartPatrolling();
    }

    void Update()
    {
        if (rb.linearVelocity.x > 0.1f) FlipSprite(true);
        else if (rb.linearVelocity.x < -0.1f) FlipSprite(false);

        // Phát hiện player khi đang trên mặt đất và không nhảy
        if (isGrounded && currentState == State.Patrolling)
        {
            if (CanSeePlayer())
            {
                StopPatrolling();
                SetState(State.Charging);
                chargeCoroutine = StartCoroutine(ChargeThenJump());
            }
        }
        if (player != null && CanSeePlayer())
        {
            Debug.DrawLine(transform.position, player.position, Color.green);
        }
        else if (player != null)
        {
            Debug.DrawLine(transform.position, player.position, Color.red);
        }
    }

    // === TUẦN TRA: NHẢY TỪNG BƯỚC NHỎ ===
    void StartPatrolling()
    {
        SetState(State.Patrolling);
        if (patrolCoroutine != null) StopCoroutine(patrolCoroutine);
        patrolCoroutine = StartCoroutine(PatrolBySmallJumps());
    }

    void StopPatrolling()
    {
        if (patrolCoroutine != null)
        {
            StopCoroutine(patrolCoroutine);
            patrolCoroutine = null;
        }
    }

    IEnumerator PatrolBySmallJumps()
    {
        while (true)
        {
            // Nghỉ sau khi tiếp đất
            yield return new WaitForSeconds(idleTime);

            // Chọn hướng
            Vector3 targetPoint = movingRight ? pointB.position : pointA.position;

            // Kiểm tra đã vượt qua điểm đích chưa?
            if (movingRight && transform.position.x >= targetPoint.x)
            {
                movingRight = false;
                continue;
            }
            else if (!movingRight && transform.position.x <= targetPoint.x)
            {
                movingRight = true;
                continue;
            }

            // Tính hướng nhảy
            float direction = movingRight ? 1f : -1f;

            // Tính vận tốc nhảy: dùng công thức đơn giản (vx, vy)
            float vx = direction * stepJumpDistance / 0.5f; // Giả sử thời gian bay ~0.5s
            float vy = stepJumpHeight; // Có thể tinh chỉnh

            rb.linearVelocity = new Vector2(vx, vy);
            FlipSprite(direction > 0);
            SetState(State.Jumping);

            // Chờ tiếp đất
            while (currentState == State.Jumping)
            {
                yield return null;
            }
        }
    }

    // === TẤN CÔNG PLAYER (giữ nguyên logic chính xác) ===
    bool CanSeePlayer()
    {
        if (player == null) return false;

        // 1. Cùng nền? (tùy chọn – bạn có thể giữ hoặc bỏ nếu dùng LOS)
        float deltaY = Mathf.Abs(player.position.y - transform.position.y);
        if (deltaY > groundTolerance) return false;

        // 2. Trong tầm?
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > detectionRange) return false;

        // 3. 👁️ Ở phía trước?
        Vector2 toPlayer = (player.position - transform.position).normalized;
        bool facingRight = originalLocalScale.x * transform.localScale.x > 0;
        Vector2 forward = facingRight ? Vector2.right : Vector2.left;
        float dot = Vector2.Dot(forward, toPlayer);
        float angle = Mathf.Acos(Mathf.Clamp(dot, -1f, 1f)) * Mathf.Rad2Deg;
        if (angle > viewAngle / 2f) return false;

        // 4. 🔍 KHÔNG BỊ CHẮN BỞI VẬT CẢN?
        RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer, distance, obstacleLayer);

        // Nếu có va chạm, và vật va chạm KHÔNG PHẢI LÀ PLAYER → bị chắn!
        if (hit.collider != null && hit.collider.gameObject != player.gameObject)
        {
            return false; // Bị vật cản che khuất
        }

        return true;
    }
    IEnumerator ChargeThenJump()
{
    Debug.Log("[Grasshopper] LẤY ĐÀ TẤN CÔNG...");
    yield return new WaitForSeconds(chargeTime);

    // 👇 Kiểm tra lại TRƯỚC KHI NHẢY
    if (isGrounded && player != null && CanSeePlayer())
    {
        Vector2 vel = CalculateAttackJumpWithFixedHeight(transform.position, player.position, stepJumpHeight);
        if (vel == Vector2.zero)
        {
            float dir = Mathf.Sign(player.position.x - transform.position.x);
            float vy = Mathf.Sqrt(2 * Mathf.Abs(Physics2D.gravity.y * rb.gravityScale) * stepJumpHeight);
            vel = new Vector2(dir * 4f, vy);
        }
        rb.linearVelocity = vel;
        FlipSprite(vel.x > 0);
        SetState(State.Jumping);
        Debug.Log("[Grasshopper] NHẢY TẤN CÔNG!");
    }
    else
    {
        // 👇 Player biến mất trong lúc lấy đà → quay lại tuần tra NGAY
        StartPatrolling();
    }
}

    // 🔥 TÍNH VẬN TỐC NHẢY TẤN CÔNG VỚI ĐỘ CAO CỐ ĐỊNH
    Vector2 CalculateAttackJumpWithFixedHeight(Vector2 startPos, Vector2 targetPos, float jumpHeight)
    {
        float gravity = Physics2D.gravity.y * rb.gravityScale; // Thường là số âm

        // Tính vy cần thiết để đạt độ cao jumpHeight
        // Công thức: h = vy^2 / (2 * |g|)  =>  vy = sqrt(2 * |g| * h)
        float vy = Mathf.Sqrt(2 * Mathf.Abs(gravity) * jumpHeight);

        // Tính thời gian bay tổng cộng (lên + xuống đến cùng độ cao)
        // t_total = 2 * vy / |g|
        float totalTime = 2 * vy / Mathf.Abs(gravity);

        // Khoảng cách ngang cần bay
        float dx = targetPos.x - startPos.x;

        // Vận tốc ngang cần có
        float vx = dx / totalTime;

        // Kiểm tra giới hạn hợp lý (tránh vx quá lớn)
        if (Mathf.Abs(vx) > 10f)
        {
            return Vector2.zero; // Không khả thi → dùng fallback
        }

        return new Vector2(vx, vy);
    }
    // === VA CHẠM ===
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            if (currentState == State.Jumping)
            {
                // 👇 LUÔN QUAY LẠI TUẦN TRA KHI TIẾP ĐẤT SAU KHI NHẢY
                SetState(State.Patrolling);
                StartCoroutine(ScanAfterLanding());
            }
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    void FlipSprite(bool faceRight)
    {
        Vector3 newScale = originalLocalScale;
        newScale.x = faceRight ? Mathf.Abs(originalLocalScale.x) : -Mathf.Abs(originalLocalScale.x);
        transform.localScale = newScale;
    }

    void SetState(State newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        Debug.Log($"[Grasshopper] Trạng thái: {currentState}");
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying) return;

        // Vẽ đoạn A-B
        Gizmos.color = Color.green;
        if (pointA != null && pointB != null)
        {
            Gizmos.DrawLine(pointA.position, pointB.position);
        }

        // Vẽ tầm nhìn (nếu có player trong scene)
        Gizmos.color = Color.yellow;
        Vector2 forward = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        float halfAngle = viewAngle * 0.5f * Mathf.Deg2Rad;
        Vector2 dir1 = new Vector2(
            Mathf.Cos(halfAngle) * forward.x - Mathf.Sin(halfAngle) * forward.y,
            Mathf.Sin(halfAngle) * forward.x + Mathf.Cos(halfAngle) * forward.y
        );
        Vector2 dir2 = new Vector2(
            Mathf.Cos(-halfAngle) * forward.x - Mathf.Sin(-halfAngle) * forward.y,
            Mathf.Sin(-halfAngle) * forward.x + Mathf.Cos(-halfAngle) * forward.y
        );

        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(dir1 * detectionRange));
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(dir2 * detectionRange));
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
    IEnumerator ScanAfterLanding()
    {
        // 1. Quét phía TRƯỚC (hướng hiện tại)
        bool facingRight = originalLocalScale.x * transform.localScale.x > 0;
        if (player != null && IsPlayerInFront())
        {
            Debug.Log("[Grasshopper] Thấy player PHÍA TRƯỚC sau khi tiếp đất!");
            yield return new WaitForSeconds(0.3f); // Nhỏ delay để ổn định
            SetState(State.Charging);
            chargeCoroutine = StartCoroutine(ChargeThenJump());
            yield break;
        }

        // 2. Quay đầu → quét phía SAU
        FlipSprite(!facingRight); // Đảo hướng nhìn
        yield return new WaitForSeconds(0.2f); // Thời gian quay đầu

        if (player != null && IsPlayerInFront()) // Giờ "phía trước" = phía sau cũ
        {
            Debug.Log("[Grasshopper] Thấy player PHÍA SAU → quay đầu tấn công!");
            yield return new WaitForSeconds(0.3f);
            SetState(State.Charging);
            chargeCoroutine = StartCoroutine(ChargeThenJump());
            yield break;
        }

        // 3. Không thấy → quay lại hướng ban đầu và tuần tra
        FlipSprite(facingRight); // Khôi phục hướng
        yield return new WaitForSeconds(0.1f);
        StartPatrolling();
    }
    bool IsPlayerInFront()
    {
        if (player == null) return false;

        // Cùng nền?
        if (Mathf.Abs(player.position.y - transform.position.y) > groundTolerance)
            return false;

        // Trong tầm?
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > detectionRange) return false;

        // Phía trước? (dựa trên hướng hiện tại)
        Vector2 toPlayer = (player.position - transform.position).normalized;
        bool currentFacingRight = originalLocalScale.x * transform.localScale.x > 0;
        Vector2 forward = currentFacingRight ? Vector2.right : Vector2.left;
        float dot = Vector2.Dot(forward, toPlayer);
        float angle = Mathf.Acos(Mathf.Clamp(dot, -1f, 1f)) * Mathf.Rad2Deg;
        if (angle > viewAngle / 2f) return false;

        // Không bị chắn?
        RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer, dist, obstacleLayer);
        return hit.collider == null || hit.collider.gameObject == player.gameObject;
    }
}