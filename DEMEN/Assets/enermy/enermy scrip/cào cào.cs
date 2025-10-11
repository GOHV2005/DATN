using UnityEngine;

public class EnemyGrasshopper : MonoBehaviour
{
    public enum State { PatrolWait, PatrolJump, Detect, PrepareAttack, Attack, Return }
    private State currentState;

    [Header("Thiết lập cơ bản")]
    public float moveSpeed = 2f;
    public float jumpForce = 3f;
    public float detectRange = 6f;
    public float attackRange = 3f;
    public Transform pointA;
    public Transform pointB;
    public Transform player;

    [Header("Ground Check")]
    public LayerMask groundLayer;

    [Header("Debug Trajectory")]
    public bool showTrajectory = true;
    public int trajectorySteps = 30;

    private Rigidbody2D rb;
    private bool facingRight = true;
    private bool isGrounded = false;
    private Vector2 patrolTarget;

    private float waitTimer;
    private float patrolTakeOffDuration = 0.5f;
    private float attackTakeOffDuration = 1f;
    private Vector2 attackTargetPos; // vị trí player cuối cùng khi nhảy

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentState = State.PatrolWait;
        patrolTarget = pointB.position;
        waitTimer = patrolTakeOffDuration;
    }

    private void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            // ----- Tuần tra chờ lấy đà -----
            case State.PatrolWait:
                if (isGrounded)
                {
                    waitTimer -= Time.deltaTime;
                    if (waitTimer <= 0)
                    {
                        currentState = State.PatrolJump;
                        rb.linearVelocity = new Vector2((patrolTarget - (Vector2)transform.position).normalized.x * moveSpeed, jumpForce);
                        Debug.Log("🦗 Nhảy tuần tra!");
                    }
                }
                break;

            case State.PatrolJump:
                if (isGrounded)
                {
                    // Nhảy xong → trở về wait
                    patrolTarget = (Vector2.Distance(transform.position, pointA.position) < Vector2.Distance(transform.position, pointB.position)) ? pointB.position : pointA.position;
                    currentState = State.PatrolWait;
                    waitTimer = patrolTakeOffDuration;
                    Debug.Log("🔁 Chạm đất, chờ lấy đà tuần tra tiếp");
                }

                // Phát hiện player
                if (distanceToPlayer < detectRange)
                {
                    currentState = State.Detect;
                    Debug.Log("🟡 Phát hiện Player!");
                }
                break;

            // ----- Phát hiện player -----
            case State.Detect:
                if (distanceToPlayer <= attackRange && isGrounded)
                {
                    currentState = State.PrepareAttack;
                    waitTimer = attackTakeOffDuration;
                    attackTargetPos = player.position; // bắt đầu lưu vị trí player
                    Debug.Log("⚡ Chuẩn bị nhảy tấn công!");
                }
                else if (distanceToPlayer > detectRange)
                {
                    currentState = State.Return;
                    Debug.Log("🔵 Quay lại tuần tra vì Player quá xa.");
                }
                break;

            // ----- Lấy đà nhảy tấn công -----
            case State.PrepareAttack:
                if (isGrounded)
                {
                    waitTimer -= Time.deltaTime;

                    // Cập nhật vị trí player trong lúc lấy đà
                    attackTargetPos = player.position;

                    if (waitTimer <= 0)
                    {
                        rb.linearVelocity = CalculateJumpToTarget(attackTargetPos);
                        currentState = State.Attack;
                        Debug.Log("🚀 Nhảy tấn công chuẩn parabol!");
                    }
                }
                break;

            // ----- Trên không, nhảy tấn công -----
            case State.Attack:
                // Khi đã rơi xuống đất
                if (isGrounded)
                {
                    if (Vector2.Distance(transform.position, player.position) <= attackRange)
                    {
                        currentState = State.PrepareAttack;
                        waitTimer = attackTakeOffDuration;
                        Debug.Log("⚡ Chuẩn bị nhảy tiếp tấn công!");
                    }
                    else if (Vector2.Distance(transform.position, player.position) > detectRange)
                    {
                        currentState = State.Return;
                        Debug.Log("🔵 Quay lại tuần tra vì Player ra ngoài detect.");
                    }
                    else
                    {
                        currentState = State.Detect;
                    }
                }
                break;

            // ----- Quay lại tuần tra -----
            case State.Return:
                if (isGrounded)
                {
                    Vector2 target = (Vector2.Distance(transform.position, pointA.position) < Vector2.Distance(transform.position, pointB.position))
                        ? pointA.position : pointB.position;
                    rb.linearVelocity = new Vector2((target - (Vector2)transform.position).normalized.x * moveSpeed, jumpForce);

                    if (Vector2.Distance(transform.position, target) < 1.5f)
                    {
                        currentState = State.PatrolWait;
                        patrolTarget = (target == (Vector2)pointA.position) ? pointB.position : pointA.position;
                        waitTimer = patrolTakeOffDuration;
                        Debug.Log("🔄 Đã về khu vực tuần tra.");
                    }
                }
                if (distanceToPlayer < detectRange)
                {
                    currentState = State.Detect;
                    Debug.Log("🟡 Player quay lại tầm nhìn, tiếp tục lấy đà.");
                }
                break;
        }

        FlipCheck();
    }

    // ----- Tính parabol nhảy tấn công -----
    private Vector2 CalculateJumpToTarget(Vector2 target)
    {
        Vector2 start = transform.position;
        float dx = target.x - start.x;
        float gravity = Mathf.Abs(Physics2D.gravity.y);

        float vy = Mathf.Sqrt(2f * gravity * jumpForce);   // chiều cao = jumpForce
        float timeToApex = vy / gravity;
        float totalTime = timeToApex * 2f;
        float vx = dx / totalTime;                         // thu hẹp theo x

        return new Vector2(vx, vy);
    }

    private void FlipCheck()
    {
        if ((rb.linearVelocity.x > 0 && !facingRight) || (rb.linearVelocity.x < 0 && facingRight))
        {
            facingRight = !facingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    // ----- Collision2D để xác định chạm đất -----
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;
            Debug.Log("✅ Enemy chạm đất");
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = false;
            Debug.Log("❌ Enemy rời đất");
        }
    }

    // ----- Debug trajectory -----
    private void OnDrawGizmosSelected()
    {
        if (showTrajectory && Application.isPlaying && (currentState == State.PrepareAttack || currentState == State.Attack) && isGrounded)
        {
            Vector2 start = transform.position;
            Vector2 vel = CalculateJumpToTarget(player.position);
            Vector2 pos = start;
            float dt = Time.fixedDeltaTime;

            Gizmos.color = Color.cyan;
            for (int i = 0; i < trajectorySteps; i++)
            {
                Vector2 nextPos = pos + vel * dt;
                vel.y -= Mathf.Abs(Physics2D.gravity.y) * dt;
                Gizmos.DrawLine(pos, nextPos);
                pos = nextPos;
            }
        }
    }
}
