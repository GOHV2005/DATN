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
        Return         // Quay lại tuần tra
    }

    [Header("=== TẦM NHÌN & KÍCH HOẠT ===")]
    public float visionRange = 6f;              // Tầm phát hiện player
    public float chargeTriggerDistance = 2f;    // Khoảng cách kích hoạt LẤY ĐÀ

    [Header("=== DI CHUYỂN ===")]
    public float moveSpeed = 2f;

    [Header("=== TẤN CÔNG LAO ===")]
    public float chargeSpeed = 8f;
    public float chargeTravelDistance = 4f;     // QUÃNG ĐƯỜNG LAO (độc lập)
    public float chargePrepareTime = 1f;
    public float stunTime = 2f;

    [Header("=== SKILL DAMAGE ===")]
    public int contactDamage = 1;  // 👈 SKILL 1: Va chạm gây 1 damage
    public int chargeDamage = 2;   // 👈 SKILL 2: Lao đầu gây 2 damage

    [Header("=== TUẦN TRA ===")]
    public Transform pointA;
    public Transform pointB;

    [Header("=== ANIMATION ===")]
    public string idleAnim = "Dung(bohung)";
    public string walkAnim = "DiBo(bohung)";
    public string prepareAnim = "layda(bohung)";
    public string chargeAnim = "chaynhanh(bohung)";
    public string stunAnim = "phanh(bohung)";

    private BeetleState currentState = BeetleState.Patrol;
    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private Transform currentTarget;
    private float stateTimer = 0f;
    private Vector2 chargeStartPos;
    private bool isCharging = false;
    private float chargeDirection = -1f; // Mặc định nhìn TRÁI → hướng lao = -1

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
        sr.flipX = false; // Sprite gốc nhìn TRÁI → flipX = false
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
    // CÁC TRẠNG THÁI HÀNH VI
    // =========================

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
            Debug.Log("🐞 [Beetle] Phát hiện Player → CHASE");
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
            Debug.Log("🐞 [Beetle] Đủ gần → LẤY ĐÀ!");
            currentState = BeetleState.PrepareCharge;
            stateTimer = chargePrepareTime;
        }
        else if (distanceToPlayer > visionRange * 1.5f)
        {
            Debug.Log("🐞 [Beetle] Mất dấu → TRỞ LẠI tuần tra");
            currentState = BeetleState.Return;
        }
    }

    void PrepareChargeBehavior()
    {
        rb.linearVelocity = Vector2.zero;
        PlayAnim(prepareAnim);

        if (stateTimer <= 0)
        {
            Debug.Log("🐞 [Beetle] Lấy đà xong → LAO TỚI!");
            currentState = BeetleState.Charge;
            chargeStartPos = transform.position;

            // Lưu hướng lao dựa trên hướng hiện tại (sprite gốc nhìn TRÁI)
            chargeDirection = sr.flipX ? 1f : -1f; // flipX=true → phải → +1

            isCharging = true;
        }
    }

    void ChargeBehavior()
    {
        if (!isCharging) return;

        PlayAnim(chargeAnim);
        rb.linearVelocity = new Vector2(chargeDirection * chargeSpeed, rb.linearVelocity.y);

        float traveled = Vector2.Distance(transform.position, chargeStartPos);
        if (traveled >= chargeTravelDistance)
        {
            Debug.Log("💥 [Beetle] Hết quãng đường lao → CHOÁNG!");
            EndCharge();
        }
    }

    void StunnedBehavior()
    {
        rb.linearVelocity = Vector2.zero;
        PlayAnim(stunAnim);

        if (stateTimer <= 0)
        {
            Debug.Log("😵 [Beetle] Hết choáng → QUAY LẠI tuần tra");
            currentState = BeetleState.Return;
        }
    }

    void ReturnBehavior()
    {
        PlayAnim(walkAnim);
        MoveTowards(currentTarget.position, moveSpeed);

        if (Vector2.Distance(transform.position, currentTarget.position) < 0.3f)
        {
            Debug.Log("🐞 [Beetle] Trở lại tuần tra");
            PlayAnim(idleAnim);
            currentState = BeetleState.Patrol;
        }
    }

    // =========================
    // VA CHẠM & GÂY SÁT THƯƠNG
    // =========================

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playerScript = other.GetComponent<PlayerController>();

            if (playerScript != null)
            {
                if (currentState == BeetleState.Charge)
                {
                    playerScript.TakeDamageFromEnemy(chargeDamage, transform.position);
                    Debug.Log($"🐞 [Beetle] Gây {chargeDamage} damage (Skill 2: Lao đầu)!");
                }
                else
                {
                    playerScript.TakeDamageFromEnemy(contactDamage, transform.position);
                    Debug.Log($"🐞 [Beetle] Gây {contactDamage} damage (Skill 1: Va chạm)!");
                }
            }
        }
    }

    // =========================
    // HÀM HỖ TRỢ
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

        // Lật sprite: sprite gốc nhìn TRÁI
        sr.flipX = (dir.x > 0); // dir.x > 0 (sang phải) → flipX = true
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

    // =========================
    // DEBUG VIZ TRONG SCENE
    // =========================

    private void OnDrawGizmos()
    {
        // 🔴 Tầm nhìn phát hiện
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        // 🟣 Khoảng cách KÍCH HOẠT LẤY ĐÀ
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, chargeTriggerDistance);

        // 🔵 Quãng đường LAO (từ vị trí bắt đầu)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, chargeTravelDistance);

        // 🟢 Đường tuần tra A-B
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(pointA.position, pointB.position);
            Gizmos.DrawSphere(pointA.position, 0.1f);
            Gizmos.DrawSphere(pointB.position, 0.1f);
        }
    }
}