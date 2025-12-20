using UnityEngine;
using System.Collections;

public class BeeAI : MonoBehaviour
{
    public enum BeeState
    {
        Hovering,
        Chasing,
        Retreating,
        KeepingDistance,
        ReturningToAnchor
    }

    [Header("=== THIẾT LẬP CƠ BẢN ===")]
    public Transform anchorPoint;
    public float hoverRadius = 3f;
    public float hoverSpeed = 1.8f;
    public float chaseSpeed = 3f;
    public float detectionRange = 5f;
    public float maxChaseDistance = 12f; // 👈 GIỚI HẠN TỐI ĐA TỪ NEO
    public float retreatDistance = 3f;
    public Vector2 keepDistanceRange = new Vector2(1.5f, 3f);
    public GameObject KimChich;

    [Header("=== SÁT THƯƠNG ===")]
    public float damage = 15f;
    public float damageCooldown = 1f;

    [Header("=== ANIMATION ===")]
    public string flyAnim = "bay(ong)";
    public string attackAnim = "TanCong(ong)";

    private BeeState currentState = BeeState.Hovering;
    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private Vector2 currentTarget;
    private float lastDamageTime = 0f;
    private bool isDead = false;
    private bool isFacingRight = false;
    private bool isAttacking = false;
    private bool isRetreatingLocked = false;
    private float retreatLockDuration = 0.5f;

    private Vector2 attackTargetPosition; // Vị trí player khi bắt đầu tấn công
    private Vector2 startAttackPosition;  // Vị trí ong khi bắt đầu tấn công (để quay về sau)
    private int attackPhase = 0;          // 0 = chưa tấn công, 1 = đang lùi, 2 = đang lao, 3 = đang quay về
    private float retreatDuration = 0.3f; // Thời gian lùi
    private float launchDuration = 0.5f;  // Thời gian lao (tùy tốc độ)

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        if (anim == null || sr == null)
        {
            Debug.LogError("[Bee] Thiếu Animator hoặc SpriteRenderer!");
            enabled = false;
            return;
        }

        if (anchorPoint == null)
            anchorPoint = transform;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb.gravityScale = 0f;

        sr.flipX = false;
        isFacingRight = false;

        SetNewRandomTarget();
        PlayAnim(flyAnim);
        KimChich.SetActive(false);

    }

    public void KimChiDoAo()
    {
        KimChich.SetActive(true);
    }
    public void KimChiDoAo1()
    {
        KimChich.SetActive(false);
    }
    void Update()
    {
        if (isDead || player == null) return;

        // 👇 KIỂM TRA GIỚI HẠN TỐI ĐA
        float distFromAnchor = Vector2.Distance(transform.position, anchorPoint.position);
        if (distFromAnchor > maxChaseDistance)
        {
            if (currentState != BeeState.Chasing && distFromAnchor > maxChaseDistance)
            {
                Debug.Log("🐝 [Ong] Quá xa tổ → Quay về!");
                currentState = BeeState.ReturningToAnchor;
                PlayAnim(flyAnim);
                isAttacking = false;
            }
        }
        else if (currentState == BeeState.ReturningToAnchor && PlayerInSight())
        {
            if (Vector2.Distance(transform.position, attackTargetPosition) < 0.5f)
            {
                Debug.Log("🐝 [Ong] Player vào tầm và đủ gần → Tiếp tục đuổi!");
                currentState = BeeState.Chasing;
                PlayAnim(attackAnim);
                isAttacking = true;
            }
        }

        if (isRetreatingLocked)
        {
            RetreatingBehavior();
            return;
        }

        switch (currentState)
        {
            case BeeState.Hovering:
                HoveringBehavior();
                break;
            case BeeState.Chasing:
                ChasingBehavior();
                break;
            case BeeState.Retreating:
                RetreatingBehavior();
                break;
            case BeeState.KeepingDistance:
                KeepingDistanceBehavior();
                break;
            case BeeState.ReturningToAnchor:
                ReturningToAnchorBehavior();
                break;
        }
    }

    void HoveringBehavior()
    {
        Vector2 dir = (currentTarget - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * hoverSpeed;
        FlipSprite(dir.x, facePlayer: false);

        if (Vector2.Distance(transform.position, currentTarget) < 0.3f)
        {
            SetNewRandomTarget();
        }

        if (PlayerInSight())
        {
            Debug.Log("🐝 [Ong] Phát hiện player → BẮT ĐẦU TẤN CÔNG KIỂU LAO!");

            // 👇 Lưu lại vị trí ong và player hiện tại
            startAttackPosition = transform.position;
            attackTargetPosition = player.position;
            attackPhase = 1; // Bắt đầu lùi

            currentState = BeeState.Chasing;
            PlayAnim(attackAnim);
            isAttacking = true;
        }
    }

    void ChasingBehavior()
    {
        if (player == null) return;

        switch (attackPhase)
        {
            case 1: // ➤ GIAI ĐOẠN 1: LÙI LẠI
                {
                    Vector2 toPlayer = (player.position - transform.position).normalized;
                    Vector2 retreatDir = -toPlayer;
                    rb.linearVelocity = retreatDir * (chaseSpeed * 0.8f);
                    FlipSprite(retreatDir.x);

                    // Bắt đầu đếm thời gian lùi → sau đó chuyển sang lao
                    StartCoroutine(DelayedLaunch());
                    // ❌ ĐỪNG đặt attackPhase = 0 ở đây!
                    // ✅ Thay vào đó, đánh dấu đã bắt đầu lùi để tránh gọi lại
                    attackPhase = 99; // hoặc dùng bool isRetreatingForAttack
                }
                break;

            case 2: // ➤ GIAI ĐOẠN 2: LAO TỚI VỊ TRÍ CỐ ĐỊNH
                {
                    Vector2 dir = (attackTargetPosition - (Vector2)transform.position).normalized;
                    rb.linearVelocity = dir * (chaseSpeed * 1.5f); // Có thể lao nhanh hơn
                    FlipSprite(dir.x);

                    // Khi đến gần điểm tấn công → chuyển sang quay về
                    if (Vector2.Distance(transform.position, attackTargetPosition) < 0.3f)
                    {
                        attackPhase = 3;
                    }
                }
                break;

            case 3: // ➤ GIAI ĐOẠN 3: QUAY VỀ VỊ TRÍ BAN ĐẦU (hoặc neo)
                {
                    Vector2 dir = (startAttackPosition - (Vector2)transform.position).normalized;
                    rb.linearVelocity = dir * hoverSpeed;
                    FlipSprite(dir.x, facePlayer: false);

                    if (Vector2.Distance(transform.position, startAttackPosition) < 0.5f)
                    {
                        // Hoàn thành → quay về hover
                        EndAttackSequence();
                    }
                }
                break;
        }
    }

    IEnumerator DelayedLaunch()
    {
        yield return new WaitForSeconds(retreatDuration);
        if (currentState == BeeState.Chasing)
        {
            attackPhase = 2; // Bắt đầu LAO
        }
    }

    void EndAttackSequence()
    {
        Debug.Log("🐝 [Ong] Hoàn thành đòn lao → Quay về hover");
        currentState = BeeState.Hovering;
        isAttacking = false;
        attackPhase = 0;
        SetNewRandomTarget();
        PlayAnim(flyAnim);
    }

    void RetreatingBehavior()
    {
        if (!player) return;

        Vector2 toPlayer = ((Vector2)player.position - (Vector2)transform.position).normalized;
        Vector2 retreatDir = Quaternion.Euler(0, 0, Random.Range(-20f, 20f)) * -toPlayer;
        rb.linearVelocity = retreatDir * (chaseSpeed * 0.8f);
        FlipSprite(retreatDir.x);

        if (Vector2.Distance(transform.position, player.position) > retreatDistance)
        {
            StartCoroutine(KeepDistanceThenAttack());
        }
    }

    void KeepingDistanceBehavior()
    {
        if (!player) return;

        float angleDeg = 60f + Random.Range(-10f, 10f);
        float angleRad = angleDeg * Mathf.Deg2Rad;
        float radius = Random.Range(keepDistanceRange.x, keepDistanceRange.y);
        Vector2 offset = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * radius;
        Vector2 targetPos = (Vector2)player.position + offset;

        Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * (hoverSpeed * 0.8f);
        FlipSprite(dir.x, facePlayer: true);
    }

    void ReturningToAnchorBehavior()
    {
        Vector2 dir = (anchorPoint.position - transform.position).normalized;
        rb.linearVelocity = dir * hoverSpeed;
        FlipSprite(dir.x, facePlayer: false);

        if (Vector2.Distance(transform.position, anchorPoint.position) < 0.5f)
        {
            currentState = BeeState.Hovering;
            SetNewRandomTarget();
        }
    }

    IEnumerator KeepDistanceThenAttack()
    {
        currentState = BeeState.KeepingDistance;
        PlayAnim(flyAnim);
        isAttacking = false;
        yield return new WaitForSeconds(3f);
        if (!isDead && player != null)
        {
            currentState = BeeState.Chasing;
            PlayAnim(attackAnim);
            isAttacking = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (isDead) return;

        if (col.CompareTag("Player") && Time.time - lastDamageTime > damageCooldown)
        {
            lastDamageTime = Time.time;
            Health hp = col.GetComponent<Health>();
            if (hp != null)
            {
                hp.TakeDamage(damage, transform);
                Debug.Log("🐝 [Ong] ĐÃ CHÍCH PLAYER!");

                if (hp.GetCurrentHealth() <= 0)
                {
                    currentState = BeeState.Hovering;
                    SetNewRandomTarget();
                    PlayAnim(flyAnim);
                    return;
                }
            }

            StartCoroutine(StartRetreatLock());
        }
    }

    IEnumerator StartRetreatLock()
    {
        isRetreatingLocked = true;
        currentState = BeeState.Retreating;
        isAttacking = false;
        PlayAnim(flyAnim);
        yield return new WaitForSeconds(retreatLockDuration);
        isRetreatingLocked = false;
    }

    void SetNewRandomTarget()
    {
        float randomAngle = Random.Range(0f, 2f * Mathf.PI);
        float randomRadius = Random.Range(0f, hoverRadius);
        float x = anchorPoint.position.x + Mathf.Cos(randomAngle) * randomRadius;
        float y = anchorPoint.position.y + Mathf.Sin(randomAngle) * randomRadius;
        currentTarget = new Vector2(x, y);
    }

    bool PlayerInSight() => player && Vector2.Distance(transform.position, player.position) <= detectionRange;

    void FlipSprite(float dirX, bool facePlayer = false)
    {
        if (facePlayer && player != null)
        {
            bool shouldFaceRight = player.position.x > transform.position.x;
            if (shouldFaceRight != isFacingRight)
            {
                isFacingRight = shouldFaceRight;
                sr.flipX = isFacingRight;
            }
        }
        else
        {
            bool shouldFaceRight = dirX > 0;
            if (shouldFaceRight != isFacingRight)
            {
                isFacingRight = shouldFaceRight;
                sr.flipX = isFacingRight;
            }
        }
    }

    void PlayAnim(string animName)
    {
        if (anim != null && anim.runtimeAnimatorController != null)
            anim.Play(animName);
    }

    private void OnDrawGizmos()
    {
        if (anchorPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(anchorPoint.position, hoverRadius);
            Gizmos.DrawSphere(anchorPoint.position, 0.1f);
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        // 👇 VẼ VÙNG GIỚI HẠN TỐI ĐA
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(anchorPoint.position, maxChaseDistance);
    }
}