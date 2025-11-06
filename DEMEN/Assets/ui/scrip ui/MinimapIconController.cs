using UnityEngine;
using UnityEngine.UI;

public class MinimapIconController : MonoBehaviour
{
    public RawImage minimapImage;       // RawImage của minimap
    public RectTransform playerIcon;    // Icon player (UI Image)
    public RectTransform spawnIconPrefab; // Prefab spawn point

    public Transform player;
    public Transform[] spawnPoints;     // Hoặc lấy từ hệ thống spawn

    public Bounds levelBounds; // Giới hạn level: min/max X,Y

    private RectTransform minimapRect;
    private RectTransform[] spawnIcons;

    void Start()
    {
        minimapRect = minimapImage.rectTransform;
        CreateSpawnIcons();
    }

    void Update()
    {
        if (player != null)
            UpdateIconPosition(playerIcon, player.position);

        for (int i = 0; i < spawnPoints.Length && i < spawnIcons.Length; i++)
        {
            if (spawnPoints[i] != null)
                UpdateIconPosition(spawnIcons[i], spawnPoints[i].position);
        }
    }

    void UpdateIconPosition(RectTransform icon, Vector3 worldPos)
    {
        // Chuẩn hóa tọa độ world → [0,1]
        float x = Mathf.InverseLerp(levelBounds.min.x, levelBounds.max.x, worldPos.x);
        float y = Mathf.InverseLerp(levelBounds.min.y, levelBounds.max.y, worldPos.y);

        // Chuyển sang tọa độ UI (-1 to 1 → -0.5 to 0.5)
        icon.anchoredPosition = new Vector2(
            (x - 0.5f) * minimapRect.sizeDelta.x,
            (y - 0.5f) * minimapRect.sizeDelta.y
        );
    }

    void CreateSpawnIcons()
    {
        spawnIcons = new RectTransform[spawnPoints.Length];
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            var icon = Instantiate(spawnIconPrefab, minimapRect);
            spawnIcons[i] = icon;
        }
    }
}