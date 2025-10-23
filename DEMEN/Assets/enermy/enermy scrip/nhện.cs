// EnemySpider.cs
using UnityEngine;
using System.Collections;

public class EnemySpider : MonoBehaviour
{
    // === VỊ TRÍ & ẨN NẤP ===
    public float maxLowerDistance = 3f;
    public float detectionRange = 4f;
    public float groundTolerance = 1f;

    // === TƠ ===
    public GameObject webPrefab;
    public float shootInterval = 1.2f;
    public int maxShots = 3; // Bắn tối đa 3 phát, sau đó rút lui (nếu muốn)
    private int shotCount = 0;

    // === COMBAT ===
    public float combatSpeed = 2.5f;
    public float combatRange = 2f;

    // === THAM CHIẾU ===
    public Transform player;
    public LayerMask obstacleLayer;

    // === THÀNH PHẦN ===
    private Vector3 originalPosition;
    private bool isGrounded = false;
    private State currentState = State.Hiding;
    private Coroutine shootCoroutine;

    private enum State { Hiding, Lowering, ShootingWeb, Combat, Returning }

    void Start()
    {
        originalPosition = transform.position;
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        // Cập nhật grounded
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 0.6f, obstacleLayer);

        switch (currentState)
        {
            case State.Hiding:
                if (CanSeePlayerFromCeiling())
                {
                    StartCoroutine(LowerDown());
                }
                break;

            case State.Combat:
                if (player != null && Vector2.Distance(transform.position, player.position) > combatRange + 1f)
                {
                    SetState(State.Returning);
                    StartCoroutine(ReturnToCeiling());
                }
                else
                {
                    ChasePlayer();
                }
                break;
        }
    }

    // === HÀNH VI CHÍNH ===

    IEnumerator LowerDown()
    {
        SetState(State.Lowering);
        Vector3 targetPos = originalPosition + Vector3.down * maxLowerDistance;

        float elapsedTime = 0f;
        float duration = 0.8f;
        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(originalPosition, targetPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;

        SetState(State.ShootingWeb);
        shootCoroutine = StartCoroutine(ShootWebLoop());
    }

    IEnumerator ShootWebLoop()
    {
        shotCount = 0;
        while (currentState == State.ShootingWeb)
        {
            // 👇 KIỂM TRA MỖI VÒNG: NẾU MẤT DẤU → RÚT LUI NGAY
            if (!CanSeePlayerFromWebPosition())
            {
                Debug.Log("[Spider] Player ra khỏi tầm → Rút lui!");
                SetState(State.Returning);
                StartCoroutine(ReturnToCeiling());
                yield break;
            }

            // Giới hạn số lần bắn (tùy chọn)
            if (maxShots > 0 && shotCount >= maxShots)
            {
                Debug.Log("[Spider] Hết số lần bắn → Rút lui!");
                SetState(State.Returning);
                StartCoroutine(ReturnToCeiling());
                yield break;
            }

            ShootWeb();
            shotCount++;
            yield return new WaitForSeconds(shootInterval);
        }
    }

    void ShootWeb()
    {
        if (webPrefab == null || player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;

        // 👇 DỊCH TƠ RA KHỎI NHỆN 0.2 ĐƠN VỊ ĐỂ TRÁNH VA CHẠM NGAY LẬP TỨC
        Vector3 spawnPos = transform.position + (Vector3)direction * 0.2f;

        GameObject web = Instantiate(webPrefab, spawnPos, Quaternion.identity);
        web.GetComponent<WebProjectile>().Initialize(direction);
        Debug.Log("[Spider] Bắn tơ!");
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
        float direction = Mathf.Sign(player.position.x - transform.position.x);
        transform.Translate(Vector2.right * direction * combatSpeed * Time.deltaTime);

        Vector3 scale = transform.localScale;
        scale.x = direction > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    IEnumerator ReturnToCeiling()
    {
        // Dừng bắn nếu đang bắn
        if (shootCoroutine != null)
        {
            StopCoroutine(shootCoroutine);
            shootCoroutine = null;
        }

        // Bỏ Rigidbody nếu có
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) Destroy(rb);

        // Leo lên
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
        SetState(State.Hiding);
    }
    // === PHÁT HIỆN PLAYER ===

    bool CanSeePlayerFromCeiling()
    {
        if (player == null) return false;

        // 👇 XOÁ điều kiện "player phải thấp hơn"

        // Chỉ kiểm tra khoảng cách ngang
        float dx = Mathf.Abs(player.position.x - transform.position.x);
        if (dx > detectionRange) return false;

        // Kiểm tra không bị chắn (Raycast)
        Vector2 dir = (player.position - transform.position).normalized;
        float dist = Vector2.Distance(transform.position, player.position);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist, obstacleLayer);

        // Cho phép va chạm với player, còn lại là vật cản
        return hit.collider == null || hit.collider.CompareTag("Player");
    }

    bool CanSeePlayerFromWebPosition()
    {
        return CanSeePlayerFromCeiling();
    }

    void SetState(State newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        Debug.Log($"[Spider] Trạng thái: {currentState}");
    }

    // ✅ VẼ GIZMOS – ĐÚNG NƠI, KHÔNG LỖI
    void OnDrawGizmos()
    {
        // Vòng phát hiện player
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Đường đu xuống
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * maxLowerDistance);
    }
}