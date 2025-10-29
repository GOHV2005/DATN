// HeartbeatEffect.cs
using UnityEngine;

public class HeartbeatEffect : MonoBehaviour
{
    [Header("References")]
    public PlayerController player;

    [Header("Heartbeat Settings")]
    public float normalScale = 1f;
    public float minPulseScale = 1.2f;   // Đập nhẹ nhất (ở ngưỡng critical)
    public float maxPulseScale = 1.8f;   // Đập mạnh nhất (khi máu = 0)
    public float minPulseSpeed = 4f;     // Tốc độ chậm nhất
    public float maxPulseSpeed = 12f;    // Tốc độ nhanh nhất
    public float criticalThreshold = 0.25f; // Bắt đầu đập khi máu ≤ 25%

    private float baseLocalScaleX;
    private float baseLocalScaleY;

    void Awake()
    {
        baseLocalScaleX = transform.localScale.x;
        baseLocalScaleY = transform.localScale.y;

        if (player == null)
        {
            player = Object.FindAnyObjectByType<PlayerController>();
            if (player == null)
                Debug.LogError("PlayerController not found!");
        }
    }

    void Update()
    {
        if (player == null || player.isDead) return;

        float healthRatio = player.CurrentHealth / player.maxHealth;

        if (healthRatio <= criticalThreshold)
        {
            // Tính "cường độ" từ 0 (25% máu) → 1 (0% máu)
            float intensity = Mathf.InverseLerp(criticalThreshold, 0f, healthRatio);

            // Điều chỉnh scale và tốc độ theo cường độ
            float currentScale = Mathf.Lerp(minPulseScale, maxPulseScale, intensity);
            float currentSpeed = Mathf.Lerp(minPulseSpeed, maxPulseSpeed, intensity);

            // Hiệu ứng đập tim
            float pulse = Mathf.Sin(Time.time * currentSpeed) * 0.1f + 1f;
            float scale = Mathf.Lerp(normalScale, currentScale, pulse);

            transform.localScale = new Vector3(
                baseLocalScaleX * scale,
                baseLocalScaleY * scale,
                1f
            );
        }
        else
        {
            // Trở về kích thước bình thường
            transform.localScale = new Vector3(baseLocalScaleX, baseLocalScaleY, 1f);
        }
    }
}