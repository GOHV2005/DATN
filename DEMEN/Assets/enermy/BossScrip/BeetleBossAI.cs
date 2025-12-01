using System.Collections;
using UnityEngine;

public class BossBeetleAI : MonoBehaviour
{
    public enum BossState { Idle, Turn, Charge, Stomp, Cooldown }

    [Header("=== PLAYER DETECTION ===")]
    public float detectionRadius = 6f;
    private Transform player;

    [Header("=== ARENA ===")]
    public Collider2D arenaBounds;  // Collider định nghĩa đấu trường

    [Header("=== CHARGE ===")]
    public float chargeSpeed = 8f;
    public float chargeDistance = 10f;
    private Vector2 chargeStartPos;
    private Vector2 chargeDirection;

    [Header("=== STOMP ===")]
    public int stonesPerStomp = 5;
    public GameObject stonePrefab;
    public float stompHeight = 5f;
    public float stompSpread = 1f;
    public float stompDelay = 0.2f;

    [Header("=== COOLDOWN ===")]
    public float cooldownTime = 2f;

    [Header("=== RAYCAST DETECTION ===")]
    public float obstacleCheckDistance = 0.5f;
    public LayerMask obstacleLayer;

    private Rigidbody2D rb;
    private BossState currentState = BossState.Idle;
    private float stateTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (!player) return;

        if (!IsPlayerInArena())
        {
            rb.linearVelocity = Vector2.zero;
            currentState = BossState.Idle;
            return;
        }

        UpdateSpriteDirection();
        UpdateSpriteDirectionByMovement();
        switch (currentState)
        {
            case BossState.Idle: IdleBehavior(); break;
            case BossState.Turn: TurnBehavior(); break;
            case BossState.Charge: ChargeBehavior(); break;
            case BossState.Stomp: break; // coroutine xử lý
            case BossState.Cooldown: CooldownBehavior(); break;
        }

        stateTimer -= Time.deltaTime;
    }


    void UpdateSpriteDirectionByMovement()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        // Lấy hướng boss đang di chuyển theo trục X
        float horizontal = rb.linearVelocity.x;

        if (horizontal > 0.01f)
            sr.flipX = true;  // đang di chuyển sang phải
        else if (horizontal < -0.01f)
            sr.flipX = false; // đang di chuyển sang trái
                              // nếu horizontal ≈ 0 → giữ hướng cũ
    }

    // --- HÀM HỖ TRỢ ---
    void UpdateSpriteDirection()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        switch (currentState)
        {
            case BossState.Charge:
                // Nhìn theo hướng lao
                if (rb.linearVelocity.x > 0.01f) sr.flipX = true;
                else if (rb.linearVelocity.x < -0.01f) sr.flipX = false;
                break;
            case BossState.Stomp:
                // Khi Stomp, nhìn theo player
                if (player != null)
                    sr.flipX = (player.position.x > transform.position.x);
                break;
            default:
                // Idle / Cooldown / Turn → giữ hướng cũ hoặc quay về player khi Turn
                if (currentState == BossState.Turn && player != null)
                    sr.flipX = (player.position.x > transform.position.x);
                break;
        }
    }



    bool IsPlayerInArena()
    {
        if (arenaBounds == null) return true; // fallback nếu không gán arena
        return arenaBounds.bounds.Contains(player.position);
    }

    #region BEHAVIOR

    void IdleBehavior()
    {
        if (Vector2.Distance(transform.position, player.position) <= detectionRadius)
        {
            currentState = BossState.Turn;
            stateTimer = 0.2f;
        }
    }

    void TurnBehavior()
    {
        chargeDirection = (player.position - transform.position).normalized;
        currentState = BossState.Charge;
        chargeStartPos = transform.position;
    }

    void ChargeBehavior()
    {
        // Chỉ di chuyển theo X
        Vector2 velocity = new Vector2(chargeDirection.x * chargeSpeed, 0f);
        rb.linearVelocity = velocity;

        // Raycast check vật cản phía trước
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right * Mathf.Sign(velocity.x), obstacleCheckDistance, obstacleLayer);
        if (hit.collider != null)
        {
            rb.linearVelocity = Vector2.zero;
            currentState = BossState.Stomp;
            StartCoroutine(StompRoutine());
            return;
        }

        // Kiểm tra quãng đường lao
        if (Vector2.Distance(chargeStartPos, transform.position) >= chargeDistance)
        {
            rb.linearVelocity = Vector2.zero;
            currentState = BossState.Cooldown;
            stateTimer = cooldownTime;
        }
    }


    IEnumerator StompRoutine()
    {
        for (int i = 0; i < stonesPerStomp; i++)
        {
            if (player == null) break;

            Vector2 currentPlayerPos = player.position;
            Vector2 offset = new Vector2(Random.Range(-stompSpread, stompSpread), stompHeight);
            Instantiate(stonePrefab, currentPlayerPos + offset, Quaternion.identity);

            yield return new WaitForSeconds(stompDelay);
        }

        currentState = BossState.Cooldown;
        stateTimer = cooldownTime;
    }

    void CooldownBehavior()
    {
        rb.linearVelocity = Vector2.zero;

        if (stateTimer <= 0f)
        {
            currentState = BossState.Turn;
            stateTimer = 0.1f;
        }
    }

    #endregion

    #region DEBUG GIZMOS

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, chargeDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stompSpread + 0.5f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(chargeDirection * obstacleCheckDistance));

        if (arenaBounds != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(arenaBounds.bounds.center, arenaBounds.bounds.size);
        }
    }

    #endregion
}
