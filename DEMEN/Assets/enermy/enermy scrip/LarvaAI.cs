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
    private bool isKnockback = false;
    private Rigidbody2D rb;

    private bool isGrounded = false;
    private float lastDamageTime = 0f;
    private Coroutine currentCoroutine;
    private bool facingRight = true;
    private Animator anim;
    private SpriteRenderer sr; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        sr = GetComponent<SpriteRenderer>(); 
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
            PlayerController player = collision.collider.GetComponent<PlayerController>();
            if (player != null)
            {
                AttackDirection dir = player.GetAttackDirection(transform.position);
                player.TakeDamage(damage, dir);
                lastDamageTime = Time.time;
            }
        }
    }

    IEnumerator MovementCycle()
    {
        while (true)
        {
            if (isKnockback)
            {
                yield return null;
                continue;
            }

            if (currentState == LarvaState.Idle)
            {
                PlayAnim(idleAnim);
                yield return new WaitForSeconds(idleTime);

                if (isKnockback) continue;

                if (facingRight && pointB != null && transform.position.x >= pointB.position.x)
                    facingRight = false;
                else if (!facingRight && pointA != null && transform.position.x <= pointA.position.x)
                    facingRight = true;

                FlipSprite(facingRight);
                Vector3 target = transform.position + (facingRight ? Vector3.right : Vector3.left) * crawlDistance;
                SetState(LarvaState.Crawl);
                currentCoroutine = StartCoroutine(CrawlTo(target));
            }
            yield return null;
        }
    }
    IEnumerator CrawlTo(Vector3 target)
    {
        PlayAnim(crawlAnim);
        Vector3 startPos = transform.position;
        float distance = Vector3.Distance(startPos, target);
        float duration = distance / crawlSpeed;
        float t = 0f;

        while (t < duration)
        {
            if (isKnockback) yield break;

            t += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, target, t / duration);
            yield return null;
        }

        transform.position = target;
        SetState(LarvaState.Idle);
    }

    void OnTakeDamage()
    {
        if (isKnockback) return;

        isKnockback = true;

        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        SetState(LarvaState.Idle);

        StartCoroutine(KnockbackRecovery());
    }

    IEnumerator KnockbackRecovery()
    {
        yield return new WaitForSeconds(0.2f); // = knockbackDuration bên Health
        isKnockback = false;
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