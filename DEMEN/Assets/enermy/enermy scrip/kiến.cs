using UnityEngine;
using System.Collections;

public class EnemyAnt : MonoBehaviour
{
    // === TUẦN TRA ===
    public Transform pointA;
    public Transform pointB;
    public float patrolSpeed = 1.5f;
    public float patrolDistance = 4f;

    // === ĐUỔI & TẤN CÔNG ===
    public float chaseSpeed = 3.5f;
    public float detectionRange = 2.5f;      // Kiến chỉ thấy gần
    public float groundTolerance = 0.3f;     // Sai khác Y tối đa

    // === PHẢN XẠ SAU KHI ĐÁNH HỤT ===
    public float scanDuration = 0.8f;        // Thời gian quét sau khi quay đầu

    // === THAM CHIẾU ===
    public Transform player;
    public LayerMask obstacleLayer;          // Layer của nền/vật cản

    // === THÀNH PHẦN ===
    private Rigidbody2D rb;
    private Vector3 originalLocalScale;
    private bool isGrounded = false;
    private bool movingRight = true;

    // === TRẠNG THÁI ===
    private enum State { Patrolling, Chasing, ScanningAfterMiss, Returning }
    private State currentState = State.Patrolling;

    private Coroutine scanCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalLocalScale = transform.localScale;

        if (pointA == null || pointB == null)
        {
            pointA = transform;
            var tempB = new GameObject("Ant_PointB");
            tempB.transform.position = transform.position + Vector3.right * patrolDistance;
            pointB = tempB.transform;
        }

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        // Flip theo hướng di chuyển
        if (rb.linearVelocity.x > 0.1f) FlipSprite(true);
        else if (rb.linearVelocity.x < -0.1f) FlipSprite(false);

        // Chỉ xử lý khi trên mặt đất
        if (!isGrounded) return;

        switch (currentState)
        {
            case State.Patrolling:
                if (CanSeePlayer())
                {
                    SetState(State.Chasing);
                }
                break;

            case State.Chasing:
                if (!CanSeePlayer())
                {
                    // Đánh hụt → quay đầu quét
                    SetState(State.ScanningAfterMiss);
                    if (scanCoroutine != null) StopCoroutine(scanCoroutine);
                    scanCoroutine = StartCoroutine(ScanAfterMiss());
                }
                break;

            case State.Returning:
                // Tự động quay lại Patrolling khi đến điểm
                break;
        }
    }

    void FixedUpdate()
    {
        if (!isGrounded) return;

        switch (currentState)
        {
            case State.Patrolling:
                Patrol();
                break;
            case State.Chasing:
                ChasePlayer();
                break;
            case State.Returning:
                ReturnToPatrolPoint();
                break;
            case State.ScanningAfterMiss:
                // Đứng im trong lúc quét
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                break;
        }
    }

    // === HÀNH VI ===
    void Patrol()
    {
        Vector3 target = movingRight ? pointB.position : pointA.position;
        float direction = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * patrolSpeed, rb.linearVelocity.y);

        if (movingRight && transform.position.x >= target.x) movingRight = false;
        else if (!movingRight && transform.position.x <= target.x) movingRight = true;
    }

    void ChasePlayer()
    {
        if (player == null) return;
        float direction = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);
    }

    void ReturnToPatrolPoint()
    {
        Vector3 nearest = Vector3.Distance(transform.position, pointA.position) < Vector3.Distance(transform.position, pointB.position)
            ? pointA.position : pointB.position;

        float direction = Mathf.Sign(nearest.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * patrolSpeed, rb.linearVelocity.y);

        if (Vector3.Distance(transform.position, nearest) < 0.1f)
        {
            SetState(State.Patrolling);
        }
    }

    IEnumerator ScanAfterMiss()
    {
        // 1. Dừng lại
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        yield return new WaitForSeconds(0.1f);

        // 2. Quay đầu
        bool wasFacingRight = originalLocalScale.x * transform.localScale.x > 0;
        FlipSprite(!wasFacingRight);
        yield return new WaitForSeconds(0.1f);

        // 3. Quét trong scanDuration
        float timer = 0;
        bool foundPlayer = false;
        while (timer < scanDuration)
        {
            if (CanSeePlayer())
            {
                foundPlayer = true;
                break;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        if (foundPlayer)
        {
            SetState(State.Chasing);
        }
        else
        {
            // Quay lại hướng ban đầu rồi tuần tra
            FlipSprite(wasFacingRight);
            yield return new WaitForSeconds(0.1f);
            SetState(State.Patrolling);
        }
    }

    // === PHÁT HIỆN PLAYER (có kiểm tra vật cản) ===
    bool CanSeePlayer()
    {
        if (player == null) return false;

        // Cùng nền?
        if (Mathf.Abs(player.position.y - transform.position.y) > groundTolerance)
            return false;

        // Trong tầm?
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > detectionRange) return false;

        // Không bị chắn?
        Vector2 direction = (player.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, dist, obstacleLayer);
        if (hit.collider != null && hit.collider.gameObject != player.gameObject)
            return false;

        return true;
    }

    // === VA CHẠM VỚI NỀN ===
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    // === LẬT SPRITE ===
    void FlipSprite(bool faceRight)
    {
        Vector3 newScale = originalLocalScale;
        newScale.x = faceRight ? Mathf.Abs(originalLocalScale.x) : -Mathf.Abs(originalLocalScale.x);
        transform.localScale = newScale;
    }

    // === QUẢN LÝ TRẠNG THÁI ===
    void SetState(State newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        Debug.Log($"[Ant] Trạng thái: {currentState}");
    }

    void OnDrawGizmos()
    {
        // Chỉ vẽ khi không đang chơi (hoặc bạn có thể cho phép khi chơi)
        // Gizmos luôn vẽ trong Scene, kể cả khi play

        // 1. VẼ PHẠM VI PHÁT HIỆN (vòng tròn xung quanh kiến)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 2. VẼ ĐƯỜNG TUẦN TRA A ↔ B
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pointA.position, pointB.position);
            Gizmos.DrawSphere(pointA.position, 0.15f);
            Gizmos.DrawSphere(pointB.position, 0.15f);
        }
        else if (Application.isPlaying == false)
        {
            // Nếu chưa gán pointA/B, vẽ điểm B ảo dựa trên patrolDistance
            Vector3 startPos = transform.position;
            Vector3 tempB = startPos + Vector3.right * patrolDistance;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(startPos, tempB);
            Gizmos.DrawSphere(startPos, 0.15f);
            Gizmos.DrawSphere(tempB, 0.15f);
        }
    }
}