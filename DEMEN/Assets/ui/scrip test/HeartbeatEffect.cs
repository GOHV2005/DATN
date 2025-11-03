// HeartbeatEffect.cs
using UnityEngine;

public class HeartbeatEffect : MonoBehaviour
{
    [Header("References")]
    public PlayerController player;

    [Header("Heartbeat Settings")]
    public float minPulseSpeed = 1.5f;   // Tốc độ chậm nhất (máu = 100%)
    public float maxPulseSpeed = 5f;     // Tốc độ nhanh nhất (máu = 0%)
    public float pulseAmplitude = 0.05f; // Biên độ đập (0.05 = 5% scale)

    private float baseLocalScaleX;
    private float baseLocalScaleY;

    void Awake()
    {
        // Lưu kích thước gốc
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
        if (player == null || player.isDead)
        {
            // Khi chết: ngừng đập, về kích thước gốc
            transform.localScale = new Vector3(baseLocalScaleX, baseLocalScaleY, 1f);
            return;
        }

        float healthRatio = player.CurrentHealth / player.maxHealth;

        if (healthRatio <= 0f)
        {
            transform.localScale = new Vector3(baseLocalScaleX, baseLocalScaleY, 1f);
            return;
        }

        // Tính cường độ: 0 (máu=100%) → 1 (máu=0%)
        float intensity = 1f - healthRatio;

        // Chỉ thay đổi tốc độ, không thay đổi biên độ tối đa
        float currentSpeed = Mathf.Lerp(minPulseSpeed, maxPulseSpeed, intensity);

        // Đập với biên độ cố định (không to lên khi máu thấp)
        float pulse = Mathf.Sin(Time.time * currentSpeed);
        float scaleOffset = pulse * pulseAmplitude;
        float currentScale = 1f + scaleOffset;

        transform.localScale = new Vector3(
            baseLocalScaleX * currentScale,
            baseLocalScaleY * currentScale,
            1f
        );
    }
}