using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SceneSaveData
{
    public string sceneName;
    public Vector3 position;
    public float playTime;

    public SceneSaveData() { }

    public SceneSaveData(string sceneName, Vector3 position, float playTime)
    {
        this.sceneName = sceneName;
        this.position = position;
        this.playTime = playTime;
    }

    public string GetPlayTimeString()
    {
        int minutes = Mathf.FloorToInt(playTime / 60f);
        int seconds = Mathf.FloorToInt(playTime % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
}

[Serializable]
public class ItemData
{
    public string itemName;
    public int quantity;
    public string spriteName;
    public string itemDescription;
    public int slotIndex;
}

[Serializable]
public class InventoryData
{
    public bool isHoldingLongden = false;
    public bool isHoldingCuocChim = false;
    public List<ItemData> items = new List<ItemData>();

    // 🧩 Thêm slotIndex để load đúng vị trí
    public void AddItem(ItemSlot slot, int slotIndex)
    {
        if (!string.IsNullOrEmpty(slot.itemName) && slot.quantity > 0)
        {
            ItemData item = new ItemData
            {
                itemName = slot.itemName,
                quantity = slot.quantity,
                itemDescription = slot.itemDescription,
                spriteName = slot.itemSprite != null ? slot.itemSprite.name : "",
                slotIndex = slotIndex
            };
            items.Add(item);
        }
    }
}

[Serializable]
public class SaveData
{
    public List<SceneSaveData> scenes = new List<SceneSaveData>();
    public InventoryData inventory = new InventoryData();

    public void AddScene(SceneSaveData scene)
    {
        var existing = scenes.Find(s => s.sceneName == scene.sceneName);
        if (existing != null)
        {
            existing.position = scene.position;
            existing.playTime = scene.playTime;
        }
        else
        {
            scenes.Add(scene);
        }
    }

    public SceneSaveData GetScene(string sceneName)
    {
        return scenes.Find(s => s.sceneName == sceneName);
    }

    // 🔥 Gọi từ SavePoint khi lưu
    public void CaptureInventory()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[SaveData] Không tìm thấy InventoryManager để lưu inventory!");
            return;
        }

        inventory = new InventoryData(); // reset trước
        var slots = InventoryManager.Instance.itemSlots;

        for (int i = 0; i < slots.Length; i++)
        {
            inventory.AddItem(slots[i], i);
        }

        Debug.Log($"[SaveData] Inventory saved với {inventory.items.Count} item(s)");
    }

    // 🔥 Dùng khi load game
    public void RestoreInventory()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[SaveData] Không tìm thấy InventoryManager để load inventory!");
            return;
        }

        InventoryManager.Instance.LoadInventoryData(inventory);
        Debug.Log("[SaveData] Inventory restored!");
    }

    public interface ISaveable
    {
        string GetUniqueID();
        object CaptureState();
        void RestoreState(object state);
    }
}
