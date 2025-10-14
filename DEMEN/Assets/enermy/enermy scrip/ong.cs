using UnityEngine;
using System.Collections;

public class BeeAI : MonoBehaviour
{
    public enum BeeState { Hovering, Chasing, Dying }

    [Header("=== THIẾT LẬP CƠ BẢN ===")]
    public Transform anchorPoint;       // Điểm neo (tổ ong)
    public float hoverRadius = 3f;      // Bán kính vùng bay
    public float hoverSpeed = 1.8f;     // Tốc độ bay lang thang
    public float chaseSpeed = 3f;       // Tốc độ đuổi
    public float detectionRange = 5f;   // Tầm phát hiện player

    [Header("=== SÁT THƯƠNG ===")]
    public float damage = 15f;
    public float damageCooldown = 1f;

    [Header("=== ANIMATION ===")]
    public string flyAnim = "bay(ong)";

    private BeeState currentState = BeeState.Hovering;
    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private Vector2 currentTarget;      // Điểm đích ngẫu nhiên
    private float lastDamageTime = 0f;
    private bool isDead = false;
    private bool isFacingRight = false;

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

        // Sprite gốc nhìn TRÁI
        sr.flipX = false;
        isFacingRight = false;

        // Chọn điểm đích đầu tiên
        SetNewRandomTarget();

        PlayAnim(flyAnim);
    }

    void Update()
    {
        if (isDead) return;

        switch (currentState)
        {
            case BeeState.Hovering:
                HoveringBehavior();
                break;
            case BeeState.Chasing:
                ChasingBehavior();
                break;
        }
    }

    void HoveringBehavior()
    {
        // Bay đến điểm đích
        Vector2 dir = (currentTarget - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * hoverSpeed;

        // Lật sprite theo hướng bay
        bool shouldFaceRight = (dir.x > 0);
        if (shouldFaceRight != isFacingRight)
        {
            isFacingRight = shouldFaceRight;
            sr.flipX = isFacingRight; // Sprite gốc TRÁI → phải thì flipX = true
        }

        // Nếu đến gần điểm đích → chọn điểm mới
        if (Vector2.Distance(transform.position, currentTarget) < 0.3f)
        {
            SetNewRandomTarget();
        }

        // Phát hiện player
        if (PlayerInSight())
        {
            Debug.Log("🐝 [Ong] Phát hiện player → ĐUỔI THEO!");
            currentState = BeeState.Chasing;
        }
    }

    void ChasingBehavior()
    {
        if (!player) return;

        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = dir * chaseSpeed;

        bool shouldFaceRight = (dir.x > 0);
        if (shouldFaceRight != isFacingRight)
        {
            isFacingRight = shouldFaceRight;
            sr.flipX = isFacingRight;
        }

        if (Vector2.Distance(transform.position, player.position) > detectionRange * 1.5f)
        {
            Debug.Log("🐝 [Ong] Mất dấu → Quay lại bay lang thang");
            currentState = BeeState.Hovering;
            SetNewRandomTarget();
        }
    }

    void SetNewRandomTarget()
    {
        // Tạo điểm ngẫu nhiên trong vòng tròn quanh anchorPoint
        float randomAngle = Random.Range(0f, 2f * Mathf.PI);
        float randomRadius = Random.Range(0f, hoverRadius);
        float x = anchorPoint.position.x + Mathf.Cos(randomAngle) * randomRadius;
        float y = anchorPoint.position.y + Mathf.Sin(randomAngle) * randomRadius;
        currentTarget = new Vector2(x, y);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (isDead) return;
        if (col.gameObject.CompareTag("Player") && Time.time - lastDamageTime > damageCooldown)
        {
            Debug.Log("🐝 [Ong] ĐÃ ĐỐT PLAYER → CHẾT!");
            col.gameObject.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            lastDamageTime = Time.time;
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        rb.gravityScale = 1f;
        rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.enabled = false;
        StartCoroutine(DestroyAfterFall());
    }

    IEnumerator DestroyAfterFall()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    bool PlayerInSight() => player && Vector2.Distance(transform.position, player.position) <= detectionRange;

    void PlayAnim(string animName)
    {
        if (anim != null)
            anim.Play(animName);
    }

    private void OnDrawGizmos()
    {
        if (anchorPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(anchorPoint.position, hoverRadius); // Vùng bay
            Gizmos.DrawSphere(anchorPoint.position, 0.1f); // Điểm neo
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange); // Tầm phát hiện
    }
}