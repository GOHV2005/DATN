using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FullMapController : MonoBehaviour
{
    public float minOrthoSize = 2f;
    public float maxOrthoSize = 30f;
    public float zoomSpeed = 3f;
    public float panSpeed = 0.5f;

    public Vector2 panLimitsMin = new Vector2(-100, -50);
    public Vector2 panLimitsMax = new Vector2(100, 50);

    private Camera cam;
    private Vector3 lastMousePosition;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        ResetView();
    }

    void LateUpdate()
    {
        HandleZoom();
        HandlePan();
        ApplyPanLimits();
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            float newSize = cam.orthographicSize - scroll * zoomSpeed * cam.orthographicSize;
            cam.orthographicSize = Mathf.Clamp(newSize, minOrthoSize, maxOrthoSize);
        }
    }

    void HandlePan()
    {
        if (Input.GetMouseButtonDown(1)) // Chuột phải
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(1)) // Đang giữ chuột phải
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            // Chuyển đổi pixel → world (phụ thuộc vào ortho size)
            float moveX = -delta.x * panSpeed * (cam.orthographicSize / 10f);
            float moveY = -delta.y * panSpeed * (cam.orthographicSize / 10f);

            transform.Translate(moveX, moveY, 0, Space.World);
            lastMousePosition = Input.mousePosition;
        }
    }

    void ApplyPanLimits()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, panLimitsMin.x, panLimitsMax.x);
        pos.y = Mathf.Clamp(pos.y, panLimitsMin.y, panLimitsMax.y);
        transform.position = pos;
    }

    public void ResetView()
    {
        // Gọi khi mở full map để reset về trung tâm
        Bounds bounds = GetWorldBoundsOfLayer(LayerMask.GetMask("FullMap"));
        if (bounds.size != Vector3.zero)
        {
            transform.position = new Vector3(bounds.center.x, bounds.center.y, -10f);
            cam.orthographicSize = Mathf.Max(
                (bounds.size.y * 0.5f) * 1.2f,
                minOrthoSize
            );
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minOrthoSize, maxOrthoSize);
        }
    }

    // Tương tự script trước — lấy bounds từ layer
    Bounds GetWorldBoundsOfLayer(LayerMask layerMask)
    {
        Bounds bounds = new Bounds();
        bool first = true;
        var renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        foreach (var r in renderers)
        {
            if (((1 << r.gameObject.layer) & layerMask) == 0) continue;
            if (first) { bounds = r.bounds; first = false; }
            else bounds.Encapsulate(r.bounds);
        }
        return bounds;
    }
}