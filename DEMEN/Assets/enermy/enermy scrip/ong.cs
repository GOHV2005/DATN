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
        ReturningToAnchor // 👈 MỚI
    }

    [Header("=== THIẾT LẬP CƠ BẢN ===")]
    public Transform anchorPoint;       // Điểm neo (tổ ong)
    public float hoverRadius = 3f;      // Bán kính vùng bay
    public float hoverSpeed = 1.8f;     // Tốc độ bay lang thang
    public float chaseSpeed = 3f;       // Tốc độ đuổi
    public float detectionRange = 5f;   // Tầm phát hiện player
    public float retreatDistance = 3f;  // Khoảng cách bay lùi sau khi chích
    public Vector2 keepDistanceRange = new Vector2(1.5f, 3f); // Khoảng cách giữ quanh player

    [Header("=== SÁT THƯƠNG ===")]
    public float damage = 15f;
    public float damageCooldown = 1f;
    [Header("=== VÙNG GIỚI HẠN ===")]
    public float maxChaseDistance = 12f; // 👈 MỚI: khoảng cách tối đa từ anchorPoint
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
    private float retreatLockDuration = 0.5f; // Thời gian khóa hướng (nửa giây)

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
    }

    void Update()
    {
        if (isDead || player == null) return;

        // 👇 Kiểm tra: nếu quá xa → quay về
        if (Vector2.Distance(transform.position, anchorPoint.position) > maxChaseDistance)
        {
            if (currentState != BeeState.ReturningToAnchor)
            {
                Debug.Log("🐝 [Ong] Quá xa tổ → Quay về!");
                currentState = BeeState.ReturningToAnchor;
                PlayAnim(flyAnim);
                isAttacking = false;
            }
        }
        // 👇 Nếu đang quay về mà player vào tầm → đuổi tiếp
        // Trong Update()
        if (currentState == BeeState.ReturningToAnchor && PlayerInSight())
        {
            // 👇 CHỈ ĐUỔI LẠI NẾU PLAYER CÒN TRONG VÙNG CHO PHÉP
            if (Vector2.Distance(player.position, anchorPoint.position) <= maxChaseDistance * 0.8f)
            {
                Debug.Log("🐝 [Ong] Player vào tầm và đủ gần → Tiếp tục đuổi!");
                currentState = BeeState.Chasing;
                PlayAnim(attackAnim);
                isAttacking = true;
            }
            // Nếu player vẫn ở rìa → tiếp tục quay về, KHÔNG ĐUỔI
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
            case BeeState.ReturningToAnchor: // 👈 THÊM
                ReturningToAnchorBehavior();
                break;
        }
    }

    void ReturningToAnchorBehavior()
    {
        Vector2 dir = (anchorPoint.position - transform.position).normalized;
        rb.linearVelocity = dir * hoverSpeed;
        FlipSprite(dir.x, facePlayer: false);

        // Khi về gần anchorPoint → quay lại trạng thái Hovering
        if (Vector2.Distance(transform.position, anchorPoint.position) < 0.5f)
        {
            currentState = BeeState.Hovering;
            SetNewRandomTarget();
        }
    }

    // 🌀 BAY LANG THANG
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
            Debug.Log("🐝 [Ong] Phát hiện player → ĐUỔI THEO!");
            currentState = BeeState.Chasing;
            PlayAnim(attackAnim);
        }
    }

    // 💢 TẤN CÔNG
    void ChasingBehavior()
    {
        if (!player) return;

        isAttacking = true;
        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = dir * chaseSpeed;
        FlipSprite(dir.x);

        // ❌ XÓA DÒNG NÀY:
        // if (Vector2.Distance(transform.position, player.position) > detectionRange * 2f)
        // {
        //     currentState = BeeState.Hovering;
        //     isAttacking = false;
        //     PlayAnim(flyAnim);
        // }
    }

    // 🏃‍♂️ BAY LÙI RA XA SAU KHI CHÍCH
    // 🏃‍♂️ BAY LÙI RA XA SAU KHI CHÍCH
    void RetreatingBehavior()
    {
        if (!player) return;

        // Bay ngược hướng player, nhưng giữ độ lệch góc nhẹ để không bay thẳng đứng
        Vector2 toPlayer = ((Vector2)player.position - (Vector2)transform.position).normalized;
        Vector2 retreatDir = Quaternion.Euler(0, 0, Random.Range(-20f, 20f)) * -toPlayer;

        rb.linearVelocity = retreatDir * (chaseSpeed * 0.8f); // bay hơi chậm hơn tí
        FlipSprite(retreatDir.x);

        // Khi đã lùi đủ xa thì chuyển sang giữ khoảng cách
        if (Vector2.Distance(transform.position, player.position) > retreatDistance)
        {
            StartCoroutine(KeepDistanceThenAttack());
        }
    }

    // 🐝 GIỮ KHOẢNG CÁCH BAY Ở GÓC 60° PHÍA TRÊN PLAYER
    void KeepingDistanceBehavior()
    {
        if (!player) return;

        // Bay quanh player ở góc 60 độ phía trên (±10 độ ngẫu nhiên)
        float angleDeg = 60f + Random.Range(-10f, 10f);
        float angleRad = angleDeg * Mathf.Deg2Rad;
        float radius = Random.Range(keepDistanceRange.x, keepDistanceRange.y);

        Vector2 offset = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * radius;
        Vector2 targetPos = (Vector2)player.position + offset;

        Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * (hoverSpeed * 0.8f); // bay chậm, nhẹ
        FlipSprite(dir.x, facePlayer: true);
    }

    // 🔁 Sau khi rút lui → giữ khoảng cách → chờ tấn công lại
    IEnumerator KeepDistanceThenAttack()
    {
        currentState = BeeState.KeepingDistance;
        PlayAnim(flyAnim);
        isAttacking = false;

        // Thời gian giữ khoảng cách cố định, không ngẫu nhiên quá lâu
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
                hp.TakeDamage(damage, transform); // Gây damage player
                Debug.Log("🐝 [Ong] ĐÃ CHÍCH PLAYER!");

                // Nếu player chết → quay lại Hovering
                if (hp.GetCurrentHealth() <= 0)
                {
                    currentState = BeeState.Hovering;
                    SetNewRandomTarget();
                    PlayAnim(flyAnim);
                    return; // Ngừng các hành động retreat/attack
                }
            }

            // Nếu player còn sống → bắt đầu retreat
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
            // Luôn nhìn về playerKeepingDistance
            bool shouldFaceRight = player.position.x > transform.position.x;
            if (shouldFaceRight != isFacingRight)
            {
                isFacingRight = shouldFaceRight;
                sr.flipX = isFacingRight;
            }
        }
        else
        {
            // Flip theo hướng di chuyển
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
    }
}
