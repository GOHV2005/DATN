using System.IO;
using UnityEngine;

public static class SaveSystem
{
    public static void SaveGame(SaveData data, int slot)
    {
        string path = Application.persistentDataPath + $"/save{slot}.json";
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log("Game Saved: " + path);
    }

    public static SaveData LoadGame(int slot)
    {
        string path = Application.persistentDataPath + $"/save{slot}.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveData>(json);
        }
        return null; // slot trống
    }

    public static void DeleteSave(int slot)
    {
        string path = Application.persistentDataPath + $"/save{slot}.json";
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Save Deleted: " + path);
        }
    }
}
