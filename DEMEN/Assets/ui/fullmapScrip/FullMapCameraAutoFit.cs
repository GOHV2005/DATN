using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FullMapCameraAutoFit : MonoBehaviour
{
    public LayerMask targetLayers; // Gán layer "FullMap"
    public float padding = 2f;     // Khoảng trống xung quanh

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        FitToContent();
    }

    public void FitToContent()
    {
        // Lấy bounds của tất cả object trong layer
        Bounds bounds = GetWorldBoundsOfLayer(targetLayers);

        if (bounds.size == Vector3.zero)
        {
            Debug.LogWarning("Không tìm thấy object nào trong layer FullMap!");
            return;
        }

        // Căn camera vào trung tâm
        transform.position = new Vector3(bounds.center.x, bounds.center.y, -10f);

        // Tính ortho size để thấy toàn bộ chiều cao
        float height = bounds.size.y + padding * 2f;
        float width = bounds.size.x + padding * 2f;

        // Tính ortho size theo chiều cao (vì camera ortho dùng height)
        cam.orthographicSize = height * 0.5f;

        // Đảm bảo chiều ngang cũng đủ (nếu level rất rộng)
        float screenRatio = (float)Screen.width / Screen.height;
        float requiredWidth = cam.orthographicSize * 2f * screenRatio;
        if (width > requiredWidth)
        {
            // Tạm bỏ qua, vì Render Texture có thể không cần đúng tỷ lệ
        }
    }

    Bounds GetWorldBoundsOfLayer(LayerMask layerMask)
    {
        Bounds bounds = new Bounds();
        bool first = true;

        var renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        int layer = layerMask.value;

        foreach (var r in renderers)
        {
            if ((layer & (1 << r.gameObject.layer)) == 0) continue;

            if (first)
            {
                bounds = r.bounds;
                first = false;
            }
            else
            {
                bounds.Encapsulate(r.bounds);
            }
        }

        return bounds;
    }
}