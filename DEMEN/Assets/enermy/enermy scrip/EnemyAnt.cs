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
    public float detectionRange = 2.5f;
    public float groundTolerance = 0.3f;
    public float damage = 20f;

    // === PHẢN XẠ SAU KHI ĐÁNH HỤT ===
    public float scanDuration = 0.8f;

    // === THAM CHIẾU ===
    public Transform player;
    public LayerMask obstacleLayer;

    // === THÀNH PHẦN ===
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector3 originalLocalScale;
    private bool isGrounded = false;
    private bool movingRight = true;

    private enum State { Patrolling, Chasing, ScanningAfterMiss, Returning, Aggressive }
    private State currentState = State.Patrolling;
    private Coroutine scanCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
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
        // 👇 XOAY DÙNG FLIPX – KHÔNG BỊ ANIMATION GHI ĐÈ
        if (rb.linearVelocity.x > 0.1f) FlipSprite(true);
        else if (rb.linearVelocity.x < -0.1f) FlipSprite(false);

        if (!isGrounded) return;

        // 👇 KIỂM TRA PLAYER CÓ CHẾT KHÔNG
        if (player != null && PlayerController.Instance != null && PlayerController.Instance.isDead)
        {
            SetState(State.Patrolling);
        }

        switch (currentState)
        {
            case State.Patrolling:
                if (CanSeePlayer()) SetState(State.Chasing);
                break;
            case State.Chasing:
                if (!CanSeePlayer())
                {
                    SetState(State.ScanningAfterMiss);
                    if (scanCoroutine != null) StopCoroutine(scanCoroutine);
                    scanCoroutine = StartCoroutine(ScanAfterMiss());
                }
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
            case State.Aggressive:
                AggressiveChase();
                break;
            case State.ScanningAfterMiss:
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                break;
        }
    }

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

    void AggressiveChase()
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
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        yield return new WaitForSeconds(0.1f);

        bool wasFacingRight = !sr.flipX;
        FlipSprite(!wasFacingRight);
        yield return new WaitForSeconds(0.1f);

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
            FlipSprite(wasFacingRight);
            yield return new WaitForSeconds(0.1f);
            SetState(State.Patrolling);
        }
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;
        if (Mathf.Abs(player.position.y - transform.position.y) > groundTolerance) return false;
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > detectionRange) return false;

        Vector2 direction = (player.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, dist, obstacleLayer);
        if (hit.collider != null && hit.collider.gameObject != player.gameObject)
            return false;

        return true;
    }

    // 👇 GỌI TỪ HEALTH KHI BỊ ĐÁNH
    /*public void OnTakeDamage()
    {
        Debug.Log("[Ant] Bị đánh! → Phản đòn!");
        //SetState(State.Aggressive);
    }*/

    // 👇 GÂY SÁT THƯƠNG KHI VA CHẠM
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && currentState == State.Aggressive)
        {
            PlayerController playerScript = other.GetComponent<PlayerController>();
            if (playerScript != null)
            {
                playerScript.TakeDamageFromEnemy(damage, transform.position);
                Debug.Log("[Ant] Gây sát thương cho Player!");
            }
        }
    }

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

    void FlipSprite(bool faceRight)
    {
        if (sr != null)
        {
            sr.flipX = faceRight; // Sprite gốc nhìn phải
        }
    }

    void SetState(State newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        Debug.Log($"[Ant] Trạng thái: {currentState}");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pointA.position, pointB.position);
            Gizmos.DrawSphere(pointA.position, 0.15f);
            Gizmos.DrawSphere(pointB.position, 0.15f);
        }
        else if (!Application.isPlaying)
        {
            Vector3 startPos = transform.position;
            Vector3 tempB = startPos + Vector3.right * patrolDistance;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(startPos, tempB);
            Gizmos.DrawSphere(startPos, 0.15f);
            Gizmos.DrawSphere(tempB, 0.15f);
        }
    }
}