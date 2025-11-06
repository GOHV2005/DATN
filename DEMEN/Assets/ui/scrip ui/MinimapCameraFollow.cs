using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MinimapCameraFollow : MonoBehaviour
{
    public Transform player;
    public float followSpeed = 5f;

    [Header("Zoom")]
    public float minOrthoSize = 3f;   // Zoom gần nhất
    public float maxOrthoSize = 20f;  // Zoom xa nhất
    public float zoomSpeed = 3f;      // Tốc độ zoom

    [Header("Level Bounds (X only for platformer)")]
    public float minX;
    public float maxX;
    public float minY = -5f;
    public float maxY = 5f;

    private Camera minimapCam;
    private Vector3 targetPosition;

    void Start()
    {
        minimapCam = GetComponent<Camera>();
        minimapCam.orthographic = true;
        // Đảm bảo ortho size nằm trong giới hạn
        minimapCam.orthographicSize = Mathf.Clamp(minimapCam.orthographicSize, minOrthoSize, maxOrthoSize);
    }

    void LateUpdate()
    {
        if (player == null) return;

        // === XỬ LÝ ZOOM ===
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            float newSize = minimapCam.orthographicSize - scroll * zoomSpeed;
            minimapCam.orthographicSize = Mathf.Clamp(newSize, minOrthoSize, maxOrthoSize);
        }

        // === XỬ LÝ FOLLOW ===
        float targetX = Mathf.Clamp(player.position.x, minX, maxX);
        float targetY = Mathf.Clamp(player.position.y, minY, maxY);
        targetPosition.Set(targetX, targetY, transform.position.z);

        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    // Optional: Vẽ vùng giới hạn trong Editor
    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying) return;

        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, 0);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 1);
        Gizmos.DrawWireCube(center, size);
    }
    // Thêm vào script (public method)
    public void ZoomIn() => AdjustZoom(-1f);
    public void ZoomOut() => AdjustZoom(1f);

    void AdjustZoom(float direction)
    {
        float newSize = minimapCam.orthographicSize + direction * zoomSpeed;
        minimapCam.orthographicSize = Mathf.Clamp(newSize, minOrthoSize, maxOrthoSize);
    }
}