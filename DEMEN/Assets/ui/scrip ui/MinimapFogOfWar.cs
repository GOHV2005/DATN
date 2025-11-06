using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class MinimapFogOfWar : MonoBehaviour, IScrollHandler
{
    public Bounds levelBounds;
    public Vector2Int fogResolution = new Vector2Int(512, 256);
    public Vector2 minMaxSize = new Vector2(100, 400); // Min/Max width (height scale theo tỷ lệ)
    public float zoomSensitivity = 0.1f;

    private Texture2D fogTexture;
    private Color[] fogPixels;
    private RawImage rawImage;
    private RectTransform rectTransform;
    private bool initialized = false;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        rectTransform = rawImage.rectTransform;
        InitializeFog();
        // Đặt kích thước ban đầu (giữa min và max)
        SetSize(250);
    }

    void InitializeFog()
    {
        fogTexture = new Texture2D(fogResolution.x, fogResolution.y, TextureFormat.RGBA32, false);
        fogPixels = new Color[fogResolution.x * fogResolution.y];

        for (int i = 0; i < fogPixels.Length; i++)
            fogPixels[i] = Color.black;

        fogTexture.SetPixels(fogPixels);
        fogTexture.Apply();
        rawImage.texture = fogTexture;
        rawImage.color = Color.white;
        initialized = true;
    }

    public void RevealArea(Vector3 worldPosition, float radius = 5f)
    {
        if (!initialized) return;

        float u = Mathf.InverseLerp(levelBounds.min.x, levelBounds.max.x, worldPosition.x);
        float v = Mathf.InverseLerp(levelBounds.min.y, levelBounds.max.y, worldPosition.y);

        int centerX = Mathf.FloorToInt(u * fogResolution.x);
        int centerY = Mathf.FloorToInt(v * fogResolution.y);
        float worldToPixelX = fogResolution.x / levelBounds.size.x;
        int radPx = Mathf.FloorToInt(radius * worldToPixelX);

        int startX = Mathf.Max(0, centerX - radPx);
        int endX = Mathf.Min(fogResolution.x - 1, centerX + radPx);
        int startY = Mathf.Max(0, centerY - radPx);
        int endY = Mathf.Min(fogResolution.y - 1, centerY + radPx);

        for (int y = startY; y <= endY; y++)
        {
            for (int x = startX; x <= endX; x++)
            {
                float dx = x - centerX;
                float dy = y - centerY;
                if (dx * dx + dy * dy <= radPx * radPx)
                {
                    int i = y * fogResolution.x + x;
                    fogPixels[i] = new Color(0, 0, 0, 0); // trong suốt = đã khám phá
                }
            }
        }

        fogTexture.SetPixels(fogPixels);
        fogTexture.Apply();
    }

    public void ResetFog()
    {
        if (!initialized) return;
        for (int i = 0; i < fogPixels.Length; i++)
            fogPixels[i] = Color.black;
        fogTexture.SetPixels(fogPixels);
        fogTexture.Apply();
    }

    // ====== ZOOM ======
    public void OnScroll(PointerEventData eventData)
    {
        float scroll = eventData.scrollDelta.y;
        float delta = scroll * zoomSensitivity;

        Vector2 currentSize = rectTransform.sizeDelta;
        float newSize = currentSize.x + delta * currentSize.x;

        SetSize(Mathf.Clamp(newSize, minMaxSize.x, minMaxSize.y));
    }

    // API để gọi từ UI (nút + / -)
    public void ZoomIn() => SetSize(rectTransform.sizeDelta.x * 1.2f);
    public void ZoomOut() => SetSize(rectTransform.sizeDelta.x * 0.8f);

    private void SetSize(float width)
    {
        float aspectRatio = (float)fogResolution.x / fogResolution.y;
        float height = width / aspectRatio;
        rectTransform.sizeDelta = new Vector2(
            Mathf.Clamp(width, minMaxSize.x, minMaxSize.y),
            Mathf.Clamp(height, minMaxSize.x / aspectRatio, minMaxSize.y / aspectRatio)
        );
    }
}