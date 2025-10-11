using UnityEngine;

public class LarvaAI : MonoBehaviour
{
    public enum LarvaState { Idle, Patrol, Chase, Attack }
    private LarvaState currentState;

    [Header("Thiết lập di chuyển")]
    public Rigidbody2D rb;
    public float moveSpeed = 2f;
    public float patrolDelay = 1f;

    [Header("Phát hiện và tấn công")]
    public Transform player;
    public float visionRange = 5f;
    public float attackRange = 1f;
    public float attackCooldown = 1.5f;

    [Header("Tuần tra")]
    public Transform pointA;
    public Transform pointB;
    private Transform currentTarget;

    private float attackTimer;
    private bool facingRight = true;

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        currentState = LarvaState.Patrol;
        currentTarget = pointA;
    }

    private void Update()
    {
        switch (currentState)
        {
            case LarvaState.Idle:
                HandleIdle();
                break;
            case LarvaState.Patrol:
                HandlePatrol();
                break;
            case LarvaState.Chase:
                HandleChase();
                break;
            case LarvaState.Attack:
                HandleAttack();
                break;
        }

        DetectPlayer();
        attackTimer -= Time.deltaTime;
    }

    // ================== STATE LOGIC ==================

    private void HandleIdle()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        Debug.Log("Trạng thái: IDLE");
    }

    private void HandlePatrol()
    {
        Debug.Log("Trạng thái: PATROL");

        float distance = Vector2.Distance(transform.position, currentTarget.position);
        if (distance < 0.2f)
        {
            // Đổi hướng tuần tra
            currentTarget = (currentTarget == pointA) ? pointB : pointA;
        }

        MoveTowards(currentTarget.position);
    }

    private void HandleChase()
    {
        Debug.Log("Trạng thái: CHASE");

        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > visionRange)
        {
            // Mất dấu player → quay lại tuần tra
            currentState = LarvaState.Patrol;
            return;
        }

        if (distanceToPlayer <= attackRange)
        {
            currentState = LarvaState.Attack;
            return;
        }

        MoveTowards(player.position);
    }

    private void HandleAttack()
    {
        Debug.Log("Trạng thái: ATTACK");

        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > attackRange)
        {
            currentState = LarvaState.Chase;
            return;
        }

        if (attackTimer <= 0)
        {
            // Giả lập hành vi cắn
            Debug.Log("Ấu trùng cắn player!");
            attackTimer = attackCooldown;
        }

        rb.linearVelocity = Vector2.zero; // Dừng lại khi tấn công
    }

    // ================== PHÁT HIỆN PLAYER ==================
    private void DetectPlayer()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= visionRange && currentState != LarvaState.Attack)
        {
            currentState = LarvaState.Chase;
        }
    }

    // ================== HÀM DI CHUYỂN ==================
    private void MoveTowards(Vector2 target)
    {
        float direction = Mathf.Sign(target.x - transform.position.x);

        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        // Đảo hướng sprite
        if ((direction > 0 && !facingRight) || (direction < 0 && facingRight))
            Flip();
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // ================== VẼ TẦM NHÌN TRONG EDITOR ==================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
    }
}
