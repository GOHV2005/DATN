// FullMapColorOverride.cs
using UnityEngine;

[ExecuteAlways]
public class FullMapColorOverride : MonoBehaviour
{
    [Header("Full Map Settings")]
    public Color fullMapColor = new Color(0.5f, 0.5f, 0.7f, 1f); // Màu nền
    public Color fullMapOutlineColor = new Color(0.2f, 0.2f, 0.4f, 1f); // Viền
    public float fullMapOutlineWidth = 0.1f; // Độ dày viền

    [Header("Objects to Override")]
    public Renderer[] targetRenderers; // Kéo các SpriteRenderer / TilemapRenderer vào đây

    private MaterialPropertyBlock mpb;

    void Start()
    {
        mpb = new MaterialPropertyBlock();
    }

    void OnWillRenderObject()
    {
        Camera cam = Camera.current;
        if (cam == null || targetRenderers == null) return;

        bool isFullMapCam = cam.CompareTag("FullMapCamera");

        foreach (var renderer in targetRenderers)
        {
            if (renderer == null) continue;

            renderer.GetPropertyBlock(mpb);

            if (isFullMapCam)
            {
                mpb.SetColor("_Color", fullMapColor);
                mpb.SetColor("_OutlineColor", fullMapOutlineColor);
                mpb.SetFloat("_OutlineWidth", fullMapOutlineWidth);
            }
            else
            {
                // Reset về mặc định
                mpb.SetColor("_Color", Color.white);
                mpb.SetColor("_OutlineColor", Color.clear);
                mpb.SetFloat("_OutlineWidth", 0f);
            }

            renderer.SetPropertyBlock(mpb);
        }
    }
}