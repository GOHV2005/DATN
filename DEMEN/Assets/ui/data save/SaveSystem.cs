using System.IO;
using UnityEngine;

public static class SaveSystem
{

    // ================== ĐƯỜNG DẪN FILE ==================
    private static string GetFilePath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, $"SaveSlot{slotIndex}.json");
    }

    private static string GetInventoryPath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, $"InventorySlot{slotIndex}.json");
    }

    // ================== LƯU GAME ==================
    public static void SaveGame(int slotId, SaveData data)
    {
        string savePath = GetFilePath(slotId);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"[SaveSystem] Saved slot {slotId} → {savePath}");

        PlayerPrefs.SetInt("LastUsedSlot", slotId);
        PlayerPrefs.Save();

    }

    // ================== TẢI GAME ==================
    public static SaveData LoadGame(int slotIndex)
    {
        string path = GetFilePath(slotIndex);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            Debug.Log($"[SaveSystem] Loaded slot {slotIndex} from {path}");
            return JsonUtility.FromJson<SaveData>(json);
        }

        Debug.LogWarning($"[SaveSystem] No save file found for slot {slotIndex}");
        return null;
    }

    // ================== XÓA SLOT ==================
    public static void DeleteSlot(int slotIndex)
    {
        string savePath = GetFilePath(slotIndex);
        string invPath = GetInventoryPath(slotIndex);

        if (File.Exists(savePath))
            File.Delete(savePath);

        if (File.Exists(invPath))
            File.Delete(invPath);

        Debug.Log($"[SaveSystem] Deleted slot {slotIndex}");
    }

    // ================== LƯU INVENTORY ==================
    public static void SaveInventory(int slotIndex, InventoryData data)
    {
        string path = GetInventoryPath(slotIndex);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log($"[SaveSystem] Inventory saved to {path} ({data.items.Count} items)");
    }

    public static InventoryData LoadInventory(int slotIndex)
    {
        string path = GetInventoryPath(slotIndex);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<InventoryData>(json);
            Debug.Log($"[SaveSystem] Inventory loaded from {path} ({data.items.Count} items)");
            return data;
        }

        Debug.LogWarning($"[SaveSystem] No inventory file found for slot {slotIndex}");
        return new InventoryData();
    }
}
