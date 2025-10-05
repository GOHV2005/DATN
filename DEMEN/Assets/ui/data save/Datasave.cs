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
    public string spriteName;      // tên sprite để load lại
    public string itemDescription;
    public int slotIndex;
}

[Serializable]
public class InventoryData
{
    public List<ItemData> items = new List<ItemData>();

    public void AddItem(ItemSlot slot)
    {
        if (!string.IsNullOrEmpty(slot.itemName) && slot.quantity > 0)
        {
            ItemData item = new ItemData
            {
                itemName = slot.itemName,
                quantity = slot.quantity,
                itemDescription = slot.itemDescription,
                spriteName = slot.itemSprite != null ? slot.itemSprite.name : ""
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

    public interface ISaveable
    {
        string GetUniqueID(); // Mỗi object 1 ID duy nhất
        object CaptureState(); // Trả về dữ liệu cần lưu
        void RestoreState(object state); // Load dữ liệu lưu
    }

}
