using UnityEngine;
using System.Collections;

public class BossBeetleAI : MonoBehaviour
{
    public enum BossState { Idle, Roar, Turn, Charge, Stunned, Stomp, Cooldown }
    private BossState currentState = BossState.Idle;

    [Header("ARENA")]
    public Collider2D arenaTrigger;
    public Transform player;

    [Header("CHARGE")]
    public float chargeSpeed = 8f;
    public float obstacleCheckDistance = 1f;
    public LayerMask obstacleLayer;

    [Header("STOMP SKILL")]
    public GameObject fallingRockPrefab;
    public int rockCount = 5;
    public float rockDelay = 0.25f;
    [Tooltip("Chiều cao rơi đá tính từ TRẦN của arena (arenaTrigger.bounds.max.y)")]
    public float rockSpawnHeight = 7f; // ← chỉnh trực tiếp trong Inspector

    [Header("SKILL PROBABILITIES")]
    [Range(0f, 1f)] public float chargeChance = 0.6f; // 60% lao, 40% đá

    [Header("TIME SETTINGS")]
    public float stunnedTime = 1.5f;
    public float cooldownTime = 1f;
    public float roarTime = 2.5f;

    [Header("CAMERA SHAKE")]
    public CameraShake cameraShake;
    public float shakeIntensity = 0.45f;

    [Header("ANIMATION")]
    public string idleAnim = "Dung(bohung)";
    public string ramAnim = "ram(bohung)";
    public string chargeAnim = "chaynhanh(bohung)";

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private Vector2 chargeDirection;
    private float stateTimer;
    private bool playerEnteredArena = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        if (!player)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        PlayAnim(idleAnim);
    }

    void Update()
    {
        if (!player) return;

        bool isInArena = arenaTrigger && arenaTrigger.bounds.Contains(player.position);

        if (!isInArena)
        {
            rb.linearVelocity = Vector2.zero;
            PlayAnim(idleAnim);
            UpdateFacingDirectionForNonMovingStates();
            return;
        }

        if (!playerEnteredArena)
        {
            playerEnteredArena = true;
            currentState = BossState.Roar;
            stateTimer = roarTime;
            PlayAnim(ramAnim);
            if (cameraShake) cameraShake.Shake(shakeIntensity, roarTime);
        }

        // Cập nhật hướng sprite
        if (currentState == BossState.Charge)
        {
            sr.flipX = (rb.linearVelocity.x > 0);
        }
        else
        {
            UpdateFacingDirectionForNonMovingStates();
        }

        // Hành vi theo trạng thái
        switch (currentState)
        {
            case BossState.Roar:
                RoarBehavior();
                break;
            case BossState.Turn:
                TurnBehavior();
                break;
            case BossState.Charge:
                ChargeBehavior();
                break;
            case BossState.Stunned:
                StunnedBehavior();
                break;
            case BossState.Stomp:
                break;
            case BossState.Cooldown:
                CooldownBehavior();
                break;
        }

        stateTimer -= Time.deltaTime;
    }

    void UpdateFacingDirectionForNonMovingStates()
    {
        sr.flipX = (player.position.x > transform.position.x);
    }

    // ========================= STATE BEHAVIORS =========================

    void RoarBehavior()
    {
        rb.linearVelocity = Vector2.zero;
        if (stateTimer <= 0)
        {
            currentState = BossState.Turn;
            stateTimer = 0.1f;
        }
    }

    void TurnBehavior()
    {
        rb.linearVelocity = Vector2.zero;
        if (stateTimer <= 0)
        {
            chargeDirection = (player.position.x > transform.position.x) ? Vector2.right : Vector2.left;
            currentState = BossState.Charge;
        }
    }

    void ChargeBehavior()
    {
        rb.linearVelocity = new Vector2(chargeDirection.x * chargeSpeed, 0f);
        PlayAnim(chargeAnim);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, chargeDirection, obstacleCheckDistance, obstacleLayer);
        if (hit.collider != null)
        {
            rb.linearVelocity = Vector2.zero;
            currentState = BossState.Stunned;
            stateTimer = stunnedTime;
        }
    }

    void StunnedBehavior()
    {
        rb.linearVelocity = Vector2.zero;
        PlayAnim(idleAnim);
        if (stateTimer <= 0)
        {
            currentState = BossState.Stomp;
            StartCoroutine(StompRoutine());
        }
    }

    IEnumerator StompRoutine()
    {
        PlayAnim(ramAnim);

        if (cameraShake)
            cameraShake.Shake(shakeIntensity, rockCount * rockDelay);

        for (int i = 0; i < rockCount; i++)
        {
            if (fallingRockPrefab && arenaTrigger)
            {
                Vector3 dropPos = new Vector3(
                    player.position.x,
                    arenaTrigger.bounds.max.y + rockSpawnHeight,
                    0
                );
                Instantiate(fallingRockPrefab, dropPos, Quaternion.identity);
            }
            yield return new WaitForSeconds(rockDelay);
        }

        currentState = BossState.Cooldown;
        stateTimer = cooldownTime;
    }

    void CooldownBehavior()
    {
        rb.linearVelocity = Vector2.zero;
        PlayAnim(idleAnim);

        if (stateTimer <= 0)
        {
            if (Random.value < chargeChance) // ← 60% mặc định
            {
                currentState = BossState.Turn;
                stateTimer = 0.1f;
            }
            else
            {
                currentState = BossState.Stomp;
                StartCoroutine(StompRoutine());
            }
        }
    }

    void PlayAnim(string animName)
    {
        if (anim != null)
            anim.Play(animName);
    }

    // ========================= GIZMOS FOR DEBUG =========================

    private void OnDrawGizmos()
    {
        // 1. Vẽ tầm nhìn lao (obstacle check)
        if (sr != null)
        {
            Gizmos.color = Color.cyan;
            Vector2 direction = sr.flipX ? Vector2.right : Vector2.left;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + direction * obstacleCheckDistance);
        }

        // 2. Vẽ chiều cao rơi đá (nếu có arenaTrigger)
        if (arenaTrigger != null)
        {
            Bounds bounds = arenaTrigger.bounds;
            float ceilingY = bounds.max.y;
            float rockSpawnY = ceilingY + rockSpawnHeight;

            // Vẽ đường trần arena
            Gizmos.color = Color.green;
            Gizmos.DrawLine(
                new Vector3(bounds.min.x, ceilingY, 0),
                new Vector3(bounds.max.x, ceilingY, 0)
            );

            // Vẽ đường vị trí rơi đá (ngang qua giữa arena)
            Gizmos.color = Color.red;
            Vector3 rockLineCenter = new Vector3((bounds.min.x + bounds.max.x) * 0.5f, rockSpawnY, 0);
            Gizmos.DrawSphere(rockLineCenter, 0.2f); // điểm trung tâm

            // Vẽ đường ngang biểu thị độ cao rơi đá
            Gizmos.DrawLine(
                new Vector3(bounds.min.x, rockSpawnY, 0),
                new Vector3(bounds.max.x, rockSpawnY, 0)
            );

            // Gắn label (không có trong Gizmos, nhưng bạn thấy rõ đường đỏ = vị trí rơi)
        }
    }
}