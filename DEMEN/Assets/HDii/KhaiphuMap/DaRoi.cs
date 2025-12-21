using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class DaRoi : MonoBehaviour
{
    [Header("Timing")]
    public float shakeDuration = 0.5f;    // thời gian rung
    public float fallDelay = 0.2f;        // delay sau rung trước khi rơi
    public float respawnTime = 0f;        // 0 = không respawn

    [Header("Shake")]
    public float shakeMagnitude = 0.05f;  // biên độ rung

    [Header("Behaviour")]
    public bool requirePlayerOnTop = true; // chỉ rơi nếu player đứng trên
    public string playerTag = "Player";

    private Rigidbody2D rb;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isFalling = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;
        rb.gravityScale = 1f;
        rb.freezeRotation = true;

        originalPosition = transform.position;
        originalRotation = transform.rotation;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null && col.isTrigger)
        {
            Debug.LogWarning($"[{name}] Collider đang là Trigger — đổi về non-trigger để OnCollisionEnter2D hoạt động.");
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isFalling) return;
        if (!collision.gameObject.CompareTag(playerTag)) return;

        if (requirePlayerOnTop)
        {
            foreach (ContactPoint2D cp in collision.contacts)
            {
                if (cp.normal.y > 0.5f)
                {
                    StartCoroutine(FallRoutine());
                    return;
                }
            }
        }
        else
        {
            StartCoroutine(FallRoutine());
        }
    }

    IEnumerator FallRoutine()
    {
        isFalling = true;

        // Rung lắc
        float t = 0f;
        Vector3 start = transform.position;
        while (t < shakeDuration)
        {
            Vector2 rand = Random.insideUnitCircle * shakeMagnitude;
            transform.position = start + (Vector3)rand;
            t += Time.deltaTime;
            yield return null;
        }

        transform.position = start;
        yield return new WaitForSeconds(fallDelay);

        // Cho block rơi
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // Respawn (nếu có)
        if (respawnTime > 0)
        {
            yield return new WaitForSeconds(respawnTime);

            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            transform.position = originalPosition;
            transform.rotation = originalRotation;
            isFalling = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.9f);
    }
}
