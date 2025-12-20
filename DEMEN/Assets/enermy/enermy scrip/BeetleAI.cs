using UnityEngine;

public class BeetleAI : MonoBehaviour
{
    public enum BeetleState
    {
        Patrol,
        Chase,
        PrepareCharge,
        Charge,
        Stunned,
        Return
    }

    [Header("=== TẦM NHÌN & KÍCH HOẠT ===")]
    public float visionRange = 6f;
    public float chargeTriggerDistance = 2f;

    [Header("=== DI CHUYỂN ===")]
    public float moveSpeed = 2f;

    [Header("=== TẤN CÔNG LAO ===")]
    public float chargeSpeed = 8f;
    public float chargeTravelDistance = 4f;
    public float chargePrepareTime = 1f;
    public float stunTime = 2f;

    [Header("=== SKILL DAMAGE ===")]
    public int contactDamage = 1;
    public int chargeDamage = 2;

    [Header("=== TUẦN TRA ===")]
    public Transform pointA;
    public Transform pointB;

    [Header("=== ANIMATION ===")]
    public string idleAnim = "Dung(bohung)";
    public string walkAnim = "DiBo(bohung)";
    public string prepareAnim = "layda(bohung)";
    public string chargeAnim = "chaynhanh(bohung)";
    public string stunAnim = "phanh(bohung)";

    [Header("=== WALL SENSOR ===")]
    public Transform groundSensor; // 👈 vẫn giữ tên nhưng giờ dùng để check WALL
    public float sensorOffset = 2f; // Khoảng cách từ tâm đến sensor (bạn muốn -2f, nhưng thực tế là +2f phía trước)
    public LayerMask wallLayer;     // ⚠️ CHỈ CHỌN LAYER "Wall" (KHÔNG bao gồm Ground)

    private BeetleState currentState = BeetleState.Patrol;
    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private Transform currentTarget;
    private float stateTimer = 0f;
    private Vector2 chargeStartPos;
    private bool isCharging = false;
    private float chargeDirection = -1f; // Mặc định nhìn TRÁI

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        if (anim == null || sr == null)
        {
            Debug.LogError("[Beetle] Thiếu Animator hoặc SpriteRenderer!");
            enabled = false;
            return;
        }

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        currentTarget = pointA ? pointA : transform;
        sr.flipX = false; // Sprite gốc nhìn TRÁI
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

        // 🔁 Cập nhật vị trí GroundSensor (dù tên là groundSensor, nhưng giờ dùng để check WALL phía trước)
        UpdateWallSensorPosition();
    }

    void UpdateWallSensorPosition()
    {
        if (groundSensor == null) return;

        // Xác định hướng trước mặt: sprite gốc nhìn TRÁI → flipX=false → hướng trước là (-1, 0)
        float lookDir = sr.flipX ? 1f : -1f; // phải = +1, trái = -1
        Vector3 sensorLocalPos = new Vector3(lookDir * sensorOffset, 0f, 0f);
        groundSensor.localPosition = sensorLocalPos;
    }

    void PatrolBehavior()
    {
        PlayAnim(walkAnim);
        MoveTowards(currentTarget.position, moveSpeed);

        if (Vector2.Distance(transform.position, currentTarget.position) < 0.2f)
        {
            currentTarget = (currentTarget == pointA) ? pointB : pointA;
            PlayAnim(idleAnim);
        }

        if (PlayerInSight())
        {
            currentState = BeetleState.Chase;
        }
    }

    void ChaseBehavior()
    {
        if (!player) return;

        PlayAnim(walkAnim);
        MoveTowards(player.position, moveSpeed * 1.5f);

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= chargeTriggerDistance)
        {
            currentState = BeetleState.PrepareCharge;
            stateTimer = chargePrepareTime;
        }
        else if (distanceToPlayer > visionRange * 1.5f)
        {
            currentState = BeetleState.Return;
        }
    }

    void PrepareChargeBehavior()
    {
        rb.linearVelocity = Vector2.zero;
        PlayAnim(prepareAnim);

        if (stateTimer <= 0)
        {
            currentState = BeetleState.Charge;
            chargeStartPos = transform.position;
            chargeDirection = sr.flipX ? 1f : -1f;
            isCharging = true;
        }
    }

    void ChargeBehavior()
    {
        if (!isCharging) return;

        PlayAnim(chargeAnim);
        rb.linearVelocity = new Vector2(chargeDirection * chargeSpeed, rb.linearVelocity.y);

        // 🔍 KIỂM TRA CHỈ WALL (KHÔNG ph
        if (IsHittingWallAhead())
        {
            Debug.Log("🐞 [Beetle] ĐÂM TRÚNG TƯỜNG → STUN!");
            EndCharge();
            return;
        }

        float traveled = Vector2.Distance(transform.position, chargeStartPos);
        if (traveled >= chargeTravelDistance)
        {
            // 🟢 Hết quãng đường → không stun, chỉ dừng (hoặc bạn có thể chọn stun nếu muốn)
            // Nhưng theo yêu cầu: CHỈ stun khi đụng tường → không stun ở đây
            isCharging = false;
            rb.linearVelocity = Vector2.zero;
            // Có thể chuyển sang Idle hoặc Return, nhưng bạn không yêu cầu → tạm dừng
            currentState = BeetleState.Patrol;
        }
    }

    void StunnedBehavior()
    {
        rb.linearVelocity = Vector2.zero;
        PlayAnim(stunAnim);

        if (stateTimer <= 0)
        {
            currentState = BeetleState.Return;
        }
    }

    void ReturnBehavior()
    {
        PlayAnim(walkAnim);
        MoveTowards(currentTarget.position, moveSpeed);

        if (Vector2.Distance(transform.position, currentTarget.position) < 0.3f)
        {
            PlayAnim(idleAnim);
            currentState = BeetleState.Patrol;
        }
    }

    // 🔒 CHỈ KIỂM TRA WALL (KHÔNG PHẢI GROUND)
    bool IsHittingWallAhead()
    {
        if (groundSensor == null) return false;

        // Dùng vị trí của sensor (đã được cập nhật theo hướng)
        Vector2 checkPos = groundSensor.position;
        // Bạn có thể dùng OverlapCircle hoặc OverlapPoint — ở đây dùng OverlapCircle nhỏ
        Collider2D hit = Physics2D.OverlapCircle(checkPos, 0.1f, wallLayer);
        return hit != null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playerScript = other.GetComponent<PlayerController>();
            if (playerScript != null)
            {
                int damage = currentState == BeetleState.Charge ? chargeDamage : contactDamage;
                playerScript.TakeDamageFromEnemy(damage, transform.position);
            }
        }
    }

    bool PlayerInSight()
    {
        return player != null && Vector2.Distance(transform.position, player.position) <= visionRange;
    }

    void MoveTowards(Vector2 target, float speed)
    {
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * speed, rb.linearVelocity.y);
        sr.flipX = (dir.x > 0); // dir.x > 0 → nhìn phải → flipX = true
    }

    void PlayAnim(string animName)
    {
        if (anim != null)
            anim.Play(animName);
    }

    void EndCharge()
    {
        isCharging = false;
        rb.linearVelocity = Vector2.zero;
        currentState = BeetleState.Stunned;
        stateTimer = stunTime;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, chargeTriggerDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, chargeTravelDistance);

        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(pointA.position, pointB.position);
            Gizmos.DrawSphere(pointA.position, 0.1f);
            Gizmos.DrawSphere(pointB.position, 0.1f);
        }

        // Hiển thị sensor ở vị trí 2f phía trước
        if (Application.isPlaying && groundSensor != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundSensor.position, 0.1f);
        }
        else if (!Application.isPlaying && sr != null)
        {
            // Dự đoán vị trí sensor trongEditMode
            float dir = (sr != null && sr.flipX) ? 1f : -1f;
            Vector3 pos = transform.position + Vector3.right * dir * sensorOffset;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(pos, 0.1f);
        }
    }
}