using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIColorCycle : MonoBehaviour
{
    [Header("Speed")]
    [Tooltip("Tốc độ đổi màu (càng lớn càng nhanh)")]
    [Range(0.1f, 5f)]
    public float speed = 1f;

    [Header("Saturation & Brightness")]
    [Range(0f, 1f)]
    public float saturation = 1f;

    [Range(0f, 1f)]
    public float value = 1f;

    private Image img;
    private float hue;

    private void Awake()
    {
        img = GetComponent<Image>();
    }

    private void Update()
    {
        hue += Time.deltaTime * speed;
        if (hue > 1f) hue -= 1f;

        Color color = Color.HSVToRGB(hue, saturation, value);
        img.color = color;
    }
}
