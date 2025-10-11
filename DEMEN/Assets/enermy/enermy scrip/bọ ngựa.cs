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

    [Header("Cooldown tấn công")]
    public float attackCooldown = 1f; // Thời gian chờ giữa các đòn
    private float lastAttackTime = -Mathf.Infinity; // Thời điểm tấn công cuối

    [Header("Tham chiếu")]
    public Transform player;

    private Rigidbody2D rb;
    private Vector2 startPos;
    private bool isAttacking = false;
    private MantisState currentState = MantisState.Idle;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
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
                AttackState(distanceToPlayer);
                break;
            case MantisState.Reveal:
                RevealState(distanceToPlayer);
                break;
            case MantisState.ReturnToIdle:
                ReturnToIdleState();
                break;
        }
    }

    void IdleState(float distance)
    {
        if (distance <= visionRange)
        {
            currentState = MantisState.Detect;
            Debug.Log("👀 [Bọ ngựa] Phát hiện con mồi!");
        }
    }

    void DetectState(float distance)
    {
        FacePlayer();

        if (distance <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            currentState = MantisState.Attack;
            Debug.Log("⚔️ [Bọ ngựa] Tấn công chớp nhoáng!");
        }
        else if (distance > visionRange)
        {
            currentState = MantisState.ReturnToIdle;
            Debug.Log("❌ [Bọ ngựa] Mất dấu con mồi, quay lại vị trí.");
        }
    }

    void AttackState(float distance)
    {
        if (isAttacking) return;

        isAttacking = true;
        lastAttackTime = Time.time; // cập nhật thời gian tấn công
        Debug.Log("💥 [Bọ ngựa] Lao ra tấn công lén!");
        FacePlayer();

        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = dir * attackSpeed;

        Invoke(nameof(EndAttack), 0.5f);
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

        if (distance > visionRange)
        {
            currentState = MantisState.ReturnToIdle;
            Debug.Log("🏃 [Bọ ngựa] Mất dấu player, quay lại vị trí.");
            return;
        }

        Vector2 moveDir = (player.position - transform.position).normalized;
        transform.position += (Vector3)moveDir * moveSpeed * Time.deltaTime;

        if (distance <= revealAttackRange && !isAttacking && Time.time >= lastAttackTime + attackCooldown)
        {
            isAttacking = true;
            lastAttackTime = Time.time;
            Debug.Log("🩸 [Bọ ngựa] Vồ tấn công trực diện!");
            rb.linearVelocity = moveDir * (attackSpeed * 1.2f);
            Invoke(nameof(EndRevealAttack), 0.4f);
        }
    }

    void EndRevealAttack()
    {
        rb.linearVelocity = Vector2.zero;
        isAttacking = false;
        Debug.Log("🔁 [Bọ ngựa] Kết thúc combo, sẵn sàng ra chiêu tiếp.");
    }

    void ReturnToIdleState()
    {
        // Quay mặt về hướng vị trí ban đầu
        if (startPos.x < transform.position.x)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        transform.position = Vector2.MoveTowards(transform.position, startPos, returnSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, startPos) < 0.1f)
        {
            currentState = MantisState.Idle;
            Debug.Log("🌿 [Bọ ngựa] Đã trở về vị trí ẩn nấp.");
        }
    }

    void FacePlayer()
    {
        if (player == null) return;

        Vector3 scale = transform.localScale;
        scale.x = player.position.x < transform.position.x ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

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
        Vector3 topLeft = new Vector3(center.x - size / 2, center.y + size / 2, 0);
        Vector3 topRight = new Vector3(center.x + size / 2, center.y + size / 2, 0);
        Vector3 bottomRight = new Vector3(center.x + size / 2, center.y - size / 2, 0);
        Vector3 bottomLeft = new Vector3(center.x - size / 2, center.y - size / 2, 0);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}
