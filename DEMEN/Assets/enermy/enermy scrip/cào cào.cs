using UnityEngine;
using System.Collections;

public class EnemyGrasshopper : MonoBehaviour
{
    // === ĐIỂM TUẦN TRA ===
    public Transform pointA;
    public Transform pointB;
    public float patrolDistance = 5f;
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
    private SpriteRenderer sr;
    private bool currentFacingRight = true;

    // 👇 BIẾN MỚI: QUẢN LÝ SCALE
    private Vector3 currentScale;
    private BoxCollider2D boxCollider;
    private float originalColliderSizeX;
    private float originalColliderSizeY;
    private float originalMass;

    private enum State { Patrolling, Charging, Jumping, Scanning }
    private State currentState = State.Patrolling;

    private Coroutine patrolCoroutine;
    private Coroutine chargeCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();

        originalLocalScale = transform.localScale;
        currentScale = originalLocalScale;

        if (boxCollider != null)
        {
            originalColliderSizeX = boxCollider.size.x;
            originalColliderSizeY = boxCollider.size.y;
        }
        if (rb != null)
        {
            originalMass = rb.mass;
        }

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
        // 👇 KIỂM TRA SCALE CÓ THAY ĐỔI KHÔNG
        if (transform.localScale != currentScale)
        {
            AdjustColliderAndMass();
            currentScale = transform.localScale;
        }

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

        // 👇 DEBUG: VẼ TIA NHÌN LUÔN (KHÔNG CẦN IF)
        if (player != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            float distance = Vector2.Distance(transform.position, player.position);

            // Tia nhìn màu xanh nếu trong tầm, đỏ nếu ngoài tầm
            if (distance <= detectionRange)
            {
                float deltaY = Mathf.Abs(player.position.y - transform.position.y);
                if (deltaY <= groundTolerance)
                {
                    Debug.DrawLine(transform.position, player.position, Color.green); // 👈 XANH: thấy rõ
                }
                else
                {
                    Debug.DrawLine(transform.position, player.position, Color.blue); // 👈 XANH LAM: cao/thấp quá
                }
            }
            else
            {
                Debug.DrawLine(transform.position, player.position, Color.red); // 👈 ĐỎ: quá xa
            }
        }

        // 👇 BỎ: KHÔNG CÒN KIỂM TRA CHỈ KHI PATROLLING
        if (isGrounded && currentState == State.Patrolling)
        {
            if (CanSeePlayer())
            {
                StopPatrolling();
                SetState(State.Charging);
                chargeCoroutine = StartCoroutine(ChargeThenJump());
            }
        }
    }

    // 👇 HÀM MỚI: ĐIỀU CHỈNH COLLIDER THEO SCALE
    void AdjustColliderAndMass()
    {
        if (boxCollider != null)
        {
            boxCollider.size = new Vector2(
                originalColliderSizeX * transform.localScale.x,
                originalColliderSizeY * transform.localScale.y
            );
        }

        if (rb != null)
        {
            rb.mass = originalMass * transform.localScale.x; // Giữ tỷ lệ khối lượng
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

        // 👇 ĐÃ BỎ: KHÔNG CÒN KIỂM TRA GÓC NHÌN
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
                // 👇 CHỈ CHUYỂN VỀ PATROLLING NẾU PLAYER KO CÒN Ở TRONG TẦM
                if (!CanSeePlayer())
                {
                    SetState(State.Patrolling);
                    StartCoroutine(ScanAfterLanding());
                }
                else
                {
                    // 👇 NẾU VẪN THẤY PLAYER → TIẾP TỤC CHỜ
                    SetState(State.Patrolling);
                    // Nếu bạn muốn tiếp tục nhảy nếu thấy player, bạn có thể bỏ qua
                    // hoặc thêm logic khác
                }
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

        // 👇 ĐÃ BỎ: KHÔNG CÒN KIỂM TRA GÓC NHÌN
        return true;
    }

    // === TIỆN ÍCH ===
    void FlipSprite(bool faceRight)
    {
        if (sr != null)
        {
            sr.flipX = faceRight; // Sprite gốc nhìn phải
            currentFacingRight = faceRight;
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

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}