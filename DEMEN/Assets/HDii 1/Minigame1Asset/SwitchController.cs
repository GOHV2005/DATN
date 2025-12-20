using UnityEngine;
using System.Collections;

public class SwitchController : MonoBehaviour
{
    public GameObject barrier;
    public float moveDistance = 5f;
    public float moveSpeed = 2f;

    private bool isActivated = false;
    private Vector3 startPos;
    private Vector3 endPos;

    void Start()
    {
        startPos = barrier.transform.position;
        endPos = startPos + Vector3.right * moveDistance;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isActivated) return;

        if (other.CompareTag("Player"))
        {
            isActivated = true;
            StartCoroutine(MoveBarrier());
        }
    }

    IEnumerator MoveBarrier()
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            barrier.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        Collider2D col = barrier.GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;
    }
}
