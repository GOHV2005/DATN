// ManaShard.cs
using UnityEngine;
using UnityEngine.UI;

public class ManaShard : MonoBehaviour
{
    [Header("Movement")]
    public float fallSpeed = 80f;           // Tốc độ rơi (pixel/s)
    public float horizontalWaveSpeed = 2f;  // Tần số lắc ngang
    public float horizontalWaveAmplitude = 30f; // Biên độ lắc (pixel)

    [Header("Lifetime")]
    public float fadeDuration = 1.2f;       // Thời gian sống + mờ
    public float startDelay = 0f;           // Delay trước khi bắt đầu

    [Header("Rotation & Scale")]
    public bool enableRotation = true;
    public float rotationSpeed = 50f;       // Độ xoay (deg/s)
    public bool enableShrink = true;
    public float minScale = 0.3f;           // Scale nhỏ nhất khi tan

    private Image image;
    private CanvasGroup canvasGroup;
    private Vector2 startPos;
    private float randomOffset; // Để mỗi mảnh lắc lệch pha

    void Awake()
    {
        image = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Init()
    {
        startPos = transform.localPosition;
        randomOffset = Random.Range(0f, Mathf.PI * 2f); // Pha ngẫu nhiên
        StartCoroutine(ShardLifecycle());
    }

    private System.Collections.IEnumerator ShardLifecycle()
    {
        yield return new WaitForSeconds(startDelay);

        float elapsed = 0f;
        float startX = startPos.x;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            // 1. Rơi xuống
            float newY = startPos.y - fallSpeed * elapsed;

            // 2. Lắc ngang kiểu sóng sin (lá rơi)
            float waveOffset = Mathf.Sin(elapsed * horizontalWaveSpeed + randomOffset) * horizontalWaveAmplitude;
            float newX = startX + waveOffset;

            transform.localPosition = new Vector2(newX, newY);

            // 3. Xoay (tuỳ chọn)
            if (enableRotation)
            {
                transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            }

            // 4. Mờ dần
            float alpha = 1f - (elapsed / fadeDuration);
            canvasGroup.alpha = alpha;

            // 5. Nhỏ dần (tuỳ chọn)
            if (enableShrink)
            {
                float scale = Mathf.Lerp(1f, minScale, elapsed / fadeDuration);
                transform.localScale = Vector3.one * scale;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}