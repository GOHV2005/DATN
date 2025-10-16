using UnityEngine;
using System.Collections;

public class EnemyGrasshopper : MonoBehaviour
{
    // === ĐIỂM TUẦN TRA ===
    public Transform pointA;
    public Transform pointB;
    public float patrolDistance = 5f;
    public float viewAngle = 120f;
    public LayerMask obstacleLayer;

    // === NHẢY TUẦN TRA ===
    public float stepJumpDistance = 1.5f;
    public float stepJumpHeight = 3f;
    public float idleTime = 0.8f;

    // === TẤN CÔNG PLAYER ===
    public float detectionRange = 6f;
    public float groundTolerance = 0.5f;
    public float chargeTime = 2f;
    public float attackJumpTime = 0.8f;

    // === ANIMATION ===
    public string idleAnim = "dung(caocao)";
    public string jumpAnim = "nhaylen(caocao)";
    public string landAnim = "roixuong(caocao)";

    // === THAM CHIẾU ===
    public Transform player;

    // === THÀNH PHẦN ===
    private Rigidbody2D rb;
    private Animator anim;
    private Vector3 originalLocalScale;
    private bool isGrounded = false;
    private bool movingRight = true;
    private SpriteRenderer sr; // 👈 THÊM
    private bool currentFacingRight = true; // 👈 THÊM

    private enum State { Patrolling, Charging, Jumping, Scanning }
    private State currentState = State.Patrolling;

    private Coroutine patrolCoroutine;
    private Coroutine chargeCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>(); // 👈 THÊM
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
        // 👇 XOAY SPRITE THEO HƯỚNG DI CHUYỂN NGANG
        if (rb.linearVelocity.x > 0.1f)
        {
            FlipSprite(true); // Nhìn phải
        }
        else if (rb.linearVelocity.x < -0.1f)
        {
            FlipSprite(false); // Nhìn trái
        }

        // 👇 CHỌN ANIMATION THEO VẬN TỐC DỌC
        if (isGrounded)
        {
            PlayAnim(idleAnim); // Đứng yên
        }
        else
        {
            if (rb.linearVelocity.y > 0.1f)
            {
                PlayAnim(jumpAnim); // Bay lên
            }
            else if (rb.linearVelocity.y < -0.1f)
            {
                PlayAnim(landAnim); // Rơi xuống
            }
        }

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

        // Debug: Vẽ tia nhìn
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
            PlayAnim(idleAnim);
            yield return new WaitForSeconds(idleTime);

            Vector3 targetPoint = movingRight ? pointB.position : pointA.position;

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

            float direction = movingRight ? 1f : -1f;
            float vx = direction * stepJumpDistance / 0.5f;
            float vy = stepJumpHeight;

            rb.linearVelocity = new Vector2(vx, vy);
            SetState(State.Jumping);

            while (currentState == State.Jumping)
            {
                yield return null;
            }
        }
    }

    // === TẤN CÔNG PLAYER ===
    bool CanSeePlayer()
    {
        if (player == null) return false;

        float deltaY = Mathf.Abs(player.position.y - transform.position.y);
        if (deltaY > groundTolerance) return false;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > detectionRange) return false;

        // 👇 LẤY HƯỚNG NHÌN HIỆN TẠI CỦA SPRITE
        bool facingRight = originalLocalScale.x * transform.localScale.x > 0;

        Vector2 forward = currentFacingRight ? Vector2.right : Vector2.left;
        Vector2 toPlayer = (player.position - transform.position).normalized;
        float dot = Vector2.Dot(forward, toPlayer);
        float angle = Mathf.Acos(Mathf.Clamp(dot, -1f, 1f)) * Mathf.Rad2Deg;
        if (angle > viewAngle / 2f) return false;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer, distance, obstacleLayer);
        if (hit.collider != null && hit.collider.gameObject != player.gameObject)
        {
            return false;
        }

        return true;
    }

    IEnumerator ChargeThenJump()
    {
        PlayAnim(idleAnim);
        Debug.Log("[Grasshopper] LẤY ĐÀ TẤN CÔNG...");
        yield return new WaitForSeconds(chargeTime);

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
            SetState(State.Jumping);
            Debug.Log("[Grasshopper] NHẢY TẤN CÔNG!");
        }
        else
        {
            StartPatrolling();
        }
    }

    Vector2 CalculateAttackJumpWithFixedHeight(Vector2 startPos, Vector2 targetPos, float jumpHeight)
    {
        float gravity = Physics2D.gravity.y * rb.gravityScale;
        float vy = Mathf.Sqrt(2 * Mathf.Abs(gravity) * jumpHeight);
        float totalTime = 2 * vy / Mathf.Abs(gravity);
        float dx = targetPos.x - startPos.x;
        float vx = dx / totalTime;

        if (Mathf.Abs(vx) > 10f)
        {
            return Vector2.zero;
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

    IEnumerator ScanAfterLanding()
    {
        PlayAnim(idleAnim);

        if (player != null && IsPlayerInFront())
        {
            Debug.Log("[Grasshopper] Thấy player PHÍA TRƯỚC sau khi tiếp đất!");
            yield return new WaitForSeconds(0.3f);
            SetState(State.Charging);
            chargeCoroutine = StartCoroutine(ChargeThenJump());
            yield break;
        }

        // 👇 DÙNG BIẾN THEO DÕI THAY VÌ TÍNH TOÁN TỪ SCALE
        bool wasFacingRight = currentFacingRight;
        FlipSprite(!wasFacingRight); // Quay đầu
        yield return new WaitForSeconds(0.2f);

        if (player != null && IsPlayerInFront())
        {
            Debug.Log("[Grasshopper] Thấy player PHÍA SAU → quay đầu tấn công!");
            yield return new WaitForSeconds(0.3f);
            SetState(State.Charging);
            chargeCoroutine = StartCoroutine(ChargeThenJump());
            yield break;
        }

        // 👇 KHÔI PHỤC HƯỚNG BAN ĐẦU
        FlipSprite(wasFacingRight);
        yield return new WaitForSeconds(0.1f);
        StartPatrolling();
    }

    bool IsPlayerInFront()
    {
        if (player == null) return false;
        if (Mathf.Abs(player.position.y - transform.position.y) > groundTolerance) return false;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > detectionRange) return false;

        Vector2 toPlayer = (player.position - transform.position).normalized;
        Vector2 forward = currentFacingRight ? Vector2.right : Vector2.left; // 👈 DÙNG BIẾN MỚI
        float dot = Vector2.Dot(forward, toPlayer);
        float angle = Mathf.Acos(Mathf.Clamp(dot, -1f, 1f)) * Mathf.Rad2Deg;
        if (angle > viewAngle / 2f) return false;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer, dist, obstacleLayer);
        return hit.collider == null || hit.collider.gameObject == player.gameObject;
    }

    // === TIỆN ÍCH ===
    void FlipSprite(bool faceRight)
    {
        if (sr != null)
        {
            sr.flipX = faceRight; // Sprite gốc nhìn phải
            currentFacingRight = faceRight; // 👈 CẬP NHẬT TRẠNG THÁI
        }
    }

    void PlayAnim(string animName)
    {
        if (anim != null)
            anim.Play(animName);
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

        Gizmos.color = Color.green;
        if (pointA != null && pointB != null)
        {
            Gizmos.DrawLine(pointA.position, pointB.position);
        }

        // Vẽ tầm nhìn theo hướng hiện tại
        Gizmos.color = Color.yellow;
        bool facingRight = originalLocalScale.x * transform.localScale.x > 0;
        Vector2 forward = facingRight ? Vector2.right : Vector2.left;
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
}