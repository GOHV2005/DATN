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
    public float damageCooldown = 1.5f;

    [Header("=== TUẦN TRA ===")]
    public Transform pointA;
    public Transform pointB;

    public string idleAnim = "nam(autrung)";
    public string crawlAnim = "let(autrung)";

    private bool isGrounded = false;
    private float lastDamageTime = 0f;
    private Coroutine currentCoroutine;
    private bool facingRight = true;
    private Animator anim;
    private SpriteRenderer sr; // 👈 THÊM

    void Start()
    {
        sr = GetComponent<SpriteRenderer>(); // 👈 THÊM
        anim = GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogError("[Larva] Thiếu component Animator!");
            enabled = false;
            return;
        }

        currentState = LarvaState.Idle;
        PlayAnim(idleAnim);

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        currentCoroutine = StartCoroutine(MovementCycle());
    }

    void Update()
    {
        CheckGrounded();
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 0.6f, obstacleLayer).collider != null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player") && Time.time - lastDamageTime > damageCooldown)
        {
            Debug.Log("[Larva] Gây sát thương!");
            collision.collider.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            lastDamageTime = Time.time;
        }
    }

    IEnumerator MovementCycle()
    {
        while (true)
        {
            if (currentState == LarvaState.Idle)
            {
                PlayAnim(idleAnim);
                yield return new WaitForSeconds(idleTime);

                if (currentState == LarvaState.Idle)
                {
                    if (facingRight && pointB != null && transform.position.x >= pointB.position.x)
                        facingRight = false;
                    else if (!facingRight && pointA != null && transform.position.x <= pointA.position.x)
                        facingRight = true;

                    FlipSprite(facingRight); // 👈 GỌI HÀM MỚI
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
        PlayAnim(crawlAnim);
        float startTime = Time.time;
        Vector3 startPos = transform.position;
        float distance = Vector3.Distance(startPos, target);
        float duration = distance / crawlSpeed;

        while (Time.time - startTime < duration)
        {
            if (currentState != LarvaState.Crawl) yield break;
            transform.position = Vector3.Lerp(startPos, target, (Time.time - startTime) / duration);
            yield return null;
        }

        transform.position = target;
        SetState(LarvaState.Idle);
    }

    // ✅ SỬA HÀM NÀY
    void FlipSprite(bool faceRight)
    {
        facingRight = faceRight;
        sr.flipX = !faceRight; // Vì sprite gốc nhìn phải
    }

    void PlayAnim(string animName)
    {
        anim.Play(animName);
    }

    void SetState(LarvaState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
    }
}