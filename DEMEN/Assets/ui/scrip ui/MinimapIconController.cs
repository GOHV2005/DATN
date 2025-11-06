using UnityEngine;
using UnityEngine.UI;

public class MinimapPlayerIcon : MonoBehaviour
{
    public RectTransform playerIcon;      // Kéo PlayerIcon vào đây
    public Transform player;              // Kéo Player vào đây
    public RectTransform minimapPanel;    // Kéo MinimapPanel (chứa RawImage) vào đây

    public float iconScale = 1f;          // Có thể dùng để scale theo zoom

    // Nếu bạn muốn minimap luôn căn giữa player → bounds không cần thiết
    // Nhưng nếu minimap có biên → cần bounds
    public bool useLevelBounds = false;
    public Vector2 minBounds = new Vector2(-50, -10);
    public Vector2 maxBounds = new Vector2(50, 10);

    void LateUpdate()
    {
        if (player == null || playerIcon == null || minimapPanel == null)
            return;

        Vector2 iconPos;
        if (playerIcon != null)
            playerIcon.anchoredPosition = Vector2.zero;
        if (useLevelBounds)
        {
            // Chuẩn hóa vị trí player về [0,1] trong bounds
            float x = Mathf.InverseLerp(minBounds.x, maxBounds.x, player.position.x);
            float y = Mathf.InverseLerp(minBounds.y, maxBounds.y, player.position.y);

            // Chuyển sang tọa độ UI (-0.5 → 0.5) → nhân với kích thước minimap
            iconPos = new Vector2(
                (x - 0.5f) * minimapPanel.sizeDelta.x,
                (y - 0.5f) * minimapPanel.sizeDelta.y
            );
        }
        else
        {
            // Giả sử minimap luôn căn giữa player → icon luôn ở (0,0)
            // Nhưng nếu minimap không căn giữa → bạn cần cách trên
            iconPos = Vector2.zero;
        }

        playerIcon.anchoredPosition = iconPos;

        // Optional: Xoay icon theo hướng nhìn của player (nếu cần)
        if (player is { } && playerIcon.TryGetComponent<RectTransform>(out var rt))
        {
            // Ví dụ: player quay mặt phải/trái → mũi tên quay
            // float angle = player.localScale.x > 0 ? 0 : 180;
            // playerIcon.localEulerAngles = new Vector3(0, 0, angle);
        }
    }
}