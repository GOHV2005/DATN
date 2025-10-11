using UnityEngine;

public class BeetleAI : MonoBehaviour
{
    public enum BeetleState
    {
        Patrol,        // Tuần tra giữa A-B
        Chase,         // Đuổi theo khi phát hiện
        PrepareCharge, // Lấy đà
        Charge,        // Lao tấn công
        Stunned,       // Choáng sau khi hụt
        Return          // Quay lại tuần tra
    }

    [Header("Thiết lập cơ bản")]
    public float visionRange = 6f;
    public float moveSpeed = 2f;
    public float chargeSpeed = 8f;
    public float chargeDistance = 4f;
    public float chargePrepareTime = 1f;
    public float stunTime = 2f;
    public Transform pointA;
    public Transform pointB;

    private BeetleState currentState = BeetleState.Patrol;
    private Transform player;
    private Rigidbody2D rb;
    private Transform currentTarget;
    private bool facingRight = true;
    private float stateTimer = 0f;
    private Vector2 chargeStartPos;
    private bool isCharging = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        currentTarget = pointA;
    }

    void Update()
    {
        switch (currentState)
        {
            case BeetleState.Patrol:
                PatrolBehavior();
                break;
            case BeetleState.Chase:
                ChaseBehavior();
                break;
            case BeetleState.PrepareCharge:
                PrepareChargeBehavior();
                break;
            case BeetleState.Charge:
                ChargeBehavior();
                break;
            case BeetleState.Stunned:
                StunnedBehavior();
                break;
            case BeetleState.Return:
                ReturnBehavior();
                break;
        }

        stateTimer -= Time.deltaTime;
    }

    // =========================
    // Các trạng thái chính
    // =========================

    void PatrolBehavior()
    {
        MoveTowards(currentTarget.position, moveSpeed);

        if (Vector2.Distance(transform.position, currentTarget.position) < 0.2f)
            currentTarget = (currentTarget == pointA) ? pointB : pointA;

        if (PlayerInSight())
        {
            Debug.Log("🐞 [Beetle] Phát hiện Player → CHASE");
            currentState = BeetleState.Chase;
        }
    }

    void ChaseBehavior()
    {
        if (!player) return;

        MoveTowards(player.position, moveSpeed * 1.5f);

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance < 2f)
        {
            Debug.Log("🐞 [Beetle] Gần Player → LẤY ĐÀ");
            currentState = BeetleState.PrepareCharge;
            stateTimer = chargePrepareTime;
        }
        else if (distance > visionRange * 1.5f)
        {
            Debug.Log("🐞 [Beetle] Mất dấu → TRỞ LẠI tuần tra");
            currentState = BeetleState.Return;
        }
    }

    void PrepareChargeBehavior()
    {
        rb.linearVelocity = Vector2.zero; // Dừng lấy đà
        if (stateTimer <= 0)
        {
            Debug.Log("🐞 [Beetle] Lấy đà xong → LAO TỚI!");
            currentState = BeetleState.Charge;
            chargeStartPos = transform.position;
            isCharging = true;
        }
    }

    void ChargeBehavior()
    {
        if (!player) return;

        if (isCharging)
        {
            float dir = facingRight ? 1f : -1f;
            rb.linearVelocity = new Vector2(dir * chargeSpeed, rb.linearVelocity.y);

            float traveled = Vector2.Distance(transform.position, chargeStartPos);
            if (traveled >= chargeDistance)
            {
                Debug.Log("💥 [Beetle] Lao hụt → BỊ CHOÁNG!");
                currentState = BeetleState.Stunned;
                stateTimer = stunTime;
                isCharging = false;
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    void StunnedBehavior()
    {
        rb.linearVelocity = Vector2.zero;
        if (stateTimer <= 0)
        {
            Debug.Log("😵 [Beetle] Hết choáng → QUAY LẠI tuần tra");
            currentState = BeetleState.Return;
        }
    }

    void ReturnBehavior()
    {
        MoveTowards(currentTarget.position, moveSpeed);
        if (Vector2.Distance(transform.position, currentTarget.position) < 0.3f)
        {
            Debug.Log("🐞 [Beetle] Trở lại tuần tra");
            currentState = BeetleState.Patrol;
        }
    }

    // =========================
    // Hỗ trợ
    // =========================

    bool PlayerInSight()
    {
        if (!player) return false;
        return Vector2.Distance(transform.position, player.position) <= visionRange;
    }

    void MoveTowards(Vector2 target, float speed)
    {
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * speed, rb.linearVelocity.y);

        if ((dir.x > 0 && !facingRight) || (dir.x < 0 && facingRight))
            Flip();
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRange);
        if (pointA && pointB)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
    }
}