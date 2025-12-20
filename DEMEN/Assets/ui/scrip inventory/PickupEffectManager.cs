using UnityEngine;

public class PickupEffectManager : MonoBehaviour
{
    public static PickupEffectManager Instance;

    [Header("UI")]
    public RectTransform inventoryIcon;
    public InventoryIconEffect iconEffect;
    public Transform flyLayer;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayPickupEffect(Sprite sprite, Vector3 worldPos)
    {
        if (sprite == null || inventoryIcon == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        // 👉 RƯƠNG MỞ + PHÓNG TO
        iconEffect?.PlayOpenEffect();

        GameObject go = new GameObject("FlyItemUI");
        go.transform.SetParent(flyLayer, false);

        FlyItemToInventory fly = go.AddComponent<FlyItemToInventory>();
        fly.Init(sprite, screenPos, inventoryIcon, OnItemArrived);
    }

    private void OnItemArrived()
    {
        // 👉 ITEM CHẠM → RƯƠNG ĐÓNG
        iconEffect?.PlayCloseEffect();
    }
}
