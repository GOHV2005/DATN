using UnityEngine;
using System.Collections;

public class FallingRock : MonoBehaviour
{
    public float fallDelay = 0.2f;     // Thời gian chờ trước khi rơi
    public float resetDelay = 3f;      // Thời gian chờ trước khi bay lại
    public float returnSpeed = 5f;     // Tốc độ bay về vị trí cũ
    public float detectCooldown = 2f;  // Thời gian nghỉ giữa 2 lần rơi

    private Rigidbody2D rb;
    private Vector3 startPos;
    private bool isFalling = false;
    private bool canTrigger = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && canTrigger && !isFalling)
        {
            StartCoroutine(FallRoutine());
        }
    }

    IEnumerator FallRoutine()
    {
        canTrigger = false;
        isFalling = true;

        yield return new WaitForSeconds(fallDelay);

        rb.bodyType = RigidbodyType2D.Dynamic; // Rơi xuống
        yield return new WaitForSeconds(resetDelay);

        // Dừng lại và quay lại chỗ cũ
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        while (Vector3.Distance(transform.position, startPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPos, returnSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = startPos;
        isFalling = false;

        // Nghỉ một chút trước khi có thể kích hoạt lại
        yield return new WaitForSeconds(detectCooldown);
        canTrigger = true;
    }
}
