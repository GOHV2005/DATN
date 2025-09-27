// SaveSystem.cs
using UnityEngine;
using System.IO;

public static class SaveSystem
{
    private static string GetPath(int slotIndex)
    {
        return $"{Application.persistentDataPath}/save{slotIndex}.json";
    }

    public static void SaveGame(SaveData data, int slotIndex)
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(GetPath(slotIndex), json);
        Debug.Log($"[SAVE] Ghi file {GetPath(slotIndex)}");
        PlayerPrefs.SetInt("CurrentSlot", slotIndex);
    }

    public static SaveData LoadGame(int slotIndex)
    {
        string path = GetPath(slotIndex);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            Debug.Log($"[LOAD] Slot {slotIndex} load thành công");
            return data;
        }
        Debug.Log($"[LOAD] Slot {slotIndex} chưa có dữ liệu.");
        return null;
    }

    public static void DeleteSlot(int slotIndex)
    {
        string path = GetPath(slotIndex);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[DELETE] Xóa slot {slotIndex} tại {path}");
        }
    }
}
