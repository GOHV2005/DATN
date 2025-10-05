using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string GetFilePath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, $"SaveSlot{slotIndex}.json");
    }

    public static void SaveGame(int slotIndex, SaveData data)
    {
        string path = GetFilePath(slotIndex);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log($"[SaveSystem] Saved slot {slotIndex} → {path}");
        InventoryManager invMgr = GameObject.Find("InventoryPanel")?.GetComponent<InventoryManager>();
        if (invMgr != null)
        {
            var invData = invMgr.GetInventoryData();
            Debug.Log($"[DEBUG SAVE] Inventory items before saving: {invData.items.Count}");
        }

    }

    public static SaveData LoadGame(int slotIndex)
    {
        string path = GetFilePath(slotIndex);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveData>(json);
        }
        return null;
    }

    public static void DeleteSlot(int slotIndex)
    {
        string path = GetFilePath(slotIndex);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[SaveSystem] Deleted slot {slotIndex}");
        }
    }
}
