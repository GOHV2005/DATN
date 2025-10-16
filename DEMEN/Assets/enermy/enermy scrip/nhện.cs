using UnityEngine;
using System.Collections;

public class EnemySpider : MonoBehaviour
{
    public float maxLowerDistance = 3f;
    public float detectionRange = 4f;
    public GameObject webPrefab;

    public Transform muzzlePoint;
    public float shootInterval = 1.2f;
    public int maxShots = 10;
    private int shotCount = 0;
    public float combatSpeed = 2.5f;
    public float combatRange = 2f;
    public string prepareShootAnim = "phinbungbandan(nhen)";
    public Transform player;
    public LayerMask obstacleLayer;
    private Rigidbody2D playerRb; // 👈 THÊM

    private Vector3 originalPosition;
    private Vector3 originalLocalScale;
    private State currentState = State.Hiding;
    private Coroutine shootCoroutine;
    private Animator anim;
    private SpriteRenderer webLineRenderer;

    private enum State { Hiding, Lowering, ShootingWeb, Combat, Returning }

    void Start()
    {
        if (player != null)
        {
            playerRb = player.GetComponent<Rigidbody2D>();
        }
        anim = GetComponent<Animator>();
        originalPosition = transform.position;
        originalLocalScale = transform.localScale;

        if (muzzlePoint == null)
            muzzlePoint = transform;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (anim != null)
            anim.enabled = false;
    }

    void Update()
    {
        if (currentState == State.Hiding && CanSeePlayerFromCeiling())
        {
            StartCoroutine(LowerDown());
        }

        if (currentState == State.Combat && player != null)
        {
            if (Vector2.Distance(transform.position, player.position) > combatRange + 1f)
            {
                SetState(State.Returning);
                StartCoroutine(ReturnToCeiling());
            }
            else
            {
                ChasePlayer();
            }
        }
    }
    Vector2 PredictPlayerPosition()
    {
        if (player == null || playerRb == null)
            return player != null ? player.position : Vector2.zero;

        // Vận tốc hiện tại của player
        Vector2 playerVelocity = playerRb.linearVelocity;

        // Khoảng cách từ nhện đến player
        float distance = Vector2.Distance(transform.position, player.position);

        // Thời gian đạn bay đến player (ước lượng)
        float timeToReach = distance / 5f; // 5f = speed của đạn

        // Dự đoán vị trí tương lai
        return (Vector2)player.position + playerVelocity * timeToReach;
    }
    IEnumerator LowerDown()
    {
        SetState(State.Lowering);

        Vector3 targetPos = originalPosition + Vector3.down * maxLowerDistance;
        float elapsedTime = 0f;
        float duration = 0.8f;

        while (elapsedTime < duration)
        {
            transform.position = Vector2.Lerp(originalPosition, targetPos, elapsedTime / duration);

            if ( webLineRenderer != null && webLineRenderer.sprite != null)
            {
                float currentLength = Vector2.Distance(originalPosition, transform.position);
                float originalHeight = webLineRenderer.sprite.rect.height / webLineRenderer.sprite.pixelsPerUnit;
                if (originalHeight <= 0) originalHeight = 1f;

                float scaleY = currentLength / originalHeight;

            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        StartCoroutine(PrepareAndShoot());
    }

    IEnumerator PrepareAndShoot()
    {
        SetState(State.ShootingWeb);

        if (anim != null)
        {
            anim.enabled = true;
            anim.Play(prepareShootAnim);
            Debug.Log($"[Spider] ▶️ Chơi animation loop: {prepareShootAnim}");
        }

        shootCoroutine = StartCoroutine(ShootWebLoop());

        yield return null; // 👈 ĐÃ SỬA: ĐẢM BẢO HÀM CÓ RETURN
    }

    public void ShootWebFromAnimation()
    {
        ShootWebInternal();
    }

    IEnumerator ShootWebLoop()
    {
        shotCount = 0;
        while (currentState == State.ShootingWeb)
        {
            if (!CanSeePlayerFromWebPosition())
            {
                SetState(State.Returning);
                StartCoroutine(ReturnToCeiling());
                yield break;
            }

            if (maxShots > 0 && shotCount >= maxShots)
            {
                SetState(State.Returning);
                StartCoroutine(ReturnToCeiling());
                yield break;
            }

            ShootWebInternal();
            shotCount++;
            yield return new WaitForSeconds(shootInterval);
        }
    }

    void ShootWebInternal()
    {
        if (webPrefab == null || player == null) return;

        // 👇 LẤY VỊ TRÍ PLAYER NGAY LÚC NÀY
        Vector2 playerPosition = player.position;
        Vector2 direction = (playerPosition - (Vector2)transform.position).normalized;

        Vector3 spawnPos = muzzlePoint.position + (Vector3)direction * 0.5f;
        GameObject web = Instantiate(webPrefab, spawnPos, Quaternion.identity);
        web.GetComponent<WebProjectile>().Initialize(direction);
        Debug.Log($"[Spider] 🔫 Bắn vào vị trí player: {playerPosition}");
    }

    public void OnWebHitPlayer()
    {
        if (currentState == State.ShootingWeb)
        {
            if (shootCoroutine != null) StopCoroutine(shootCoroutine);
            SetState(State.Combat);
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 2f;
            rb.freezeRotation = true;
        }
    }

    void ChasePlayer()
    {
        if (player == null) return;
        float dir = Mathf.Sign(player.position.x - transform.position.x);
        transform.Translate(Vector2.right * dir * combatSpeed * Time.deltaTime);

        Vector3 scale = transform.localScale;
        scale.x = dir > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    IEnumerator ReturnToCeiling()
    {
        anim.Play("xoaynguoi(nhen)");
        if (shootCoroutine != null)
        {
            StopCoroutine(shootCoroutine);
            shootCoroutine = null;
        }


        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) Destroy(rb);

        float elapsedTime = 0f;
        float duration = 1f;
        Vector3 startPos = transform.position;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPos, originalPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
        transform.localScale = originalLocalScale;
        SetState(State.Hiding);

        if (anim != null)
            anim.enabled = false;

        Debug.Log("[Spider] 🕸️ Đã quay về trần, reset sprite");
    }

    bool CanSeePlayerFromCeiling()
    {
        if (player == null) return false;
        if (Mathf.Abs(player.position.x - transform.position.x) > detectionRange) return false;

        Vector2 dir = (player.position - transform.position).normalized;
        float dist = Vector2.Distance(transform.position, player.position);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist, obstacleLayer);
        return hit.collider == null || hit.collider.CompareTag("Player");
    }

    bool CanSeePlayerFromWebPosition() => CanSeePlayerFromCeiling();

    void SetState(State newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        Debug.Log($"[Spider] 🔄 Trạng thái: {newState}");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * maxLowerDistance);
        if (muzzlePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(muzzlePoint.position, 0.1f);
        }
    }
}