using UnityEngine;
using System.Collections;

public class LarvaAI : MonoBehaviour
{
    public enum LarvaState { Idle, Crawl }
    private LarvaState currentState;

    [Header("=== CÀI ĐẶT ===")]
    public Transform player;
    public LayerMask obstacleLayer;

    [Header("=== DI CHUYỂN ===")]
    public float crawlSpeed = 1.8f;
    public float crawlDistance = 1.5f;
    public float idleTime = 1.0f;

    [Header("=== SÁT THƯƠNG ===")]
    public float damage = 10f;
    public float damageCooldown = 1.5f; // Thời gian giữa các lần gây sát thương

    [Header("=== TUẦN TRA ===")]
    public Transform pointA;
    public Transform pointB;

    private bool isGrounded = false;
    private float lastDamageTime = 0f;
    private Coroutine currentCoroutine;
    private bool facingRight = true;

    void Start()
    {
        currentState = LarvaState.Idle;
        currentCoroutine = StartCoroutine(MovementCycle());
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    void Update()
    {
        CheckGrounded();
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 0.6f, obstacleLayer).collider != null;
    }

    // ================== GÂY SÁT THƯƠNG KHI CHẠM ==================
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player") && Time.time - lastDamageTime > damageCooldown)
        {
            Debug.Log("[Larva] Player chạm vào → gây sát thương!");
            // 🩸 GÂY SÁT THƯƠNG
            collision.collider.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            lastDamageTime = Time.time;
        }
    }
    

    // ================== TUẦN TRA ==================
    IEnumerator MovementCycle()
    {
        while (true)
        {
            if (currentState == LarvaState.Idle)
            {
                yield return new WaitForSeconds(idleTime);
                if (currentState == LarvaState.Idle)
                {
                    if (facingRight && transform.position.x >= pointB.position.x)
                        facingRight = false;
                    else if (!facingRight && transform.position.x <= pointA.position.x)
                        facingRight = true;

                    FlipSprite(facingRight);
                    Vector3 target = transform.position + (facingRight ? Vector3.right : Vector3.left) * crawlDistance;
                    SetState(LarvaState.Crawl);
                    StartCoroutine(CrawlTo(target));
                }
            }
            yield return null;
        }
    }

    IEnumerator CrawlTo(Vector3 target)
    {
        float startTime = Time.time;
        Vector3 startPos = transform.position;
        float duration = Vector3.Distance(startPos, target) / crawlSpeed;

        while (Time.time - startTime < duration)
        {
            if (currentState != LarvaState.Crawl) yield break;
            transform.position = Vector3.Lerp(startPos, target, (Time.time - startTime) / duration);
            yield return null;
        }
        SetState(LarvaState.Idle);
    }

    // ================== TIỆN ÍCH ==================
    void FlipSprite(bool faceRight)
    {
        facingRight = faceRight;
        Vector3 scale = transform.localScale;
        scale.x = faceRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    void SetState(LarvaState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        if (newState == LarvaState.Idle && currentCoroutine == null)
        {
            currentCoroutine = StartCoroutine(MovementCycle());
        }
    }

    // ================== DEBUG ==================
    void OnDrawGizmosSelected()
    {
        // Vòng va chạm (phụ thuộc vào Collider2D của bạn)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.3f); // Ước lượng
    }
}