using UnityEngine;

public static class SaveSystem
{
    public static void SaveGame(int slotIndex, SaveData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SaveSlot" + slotIndex, json);
        PlayerPrefs.Save();
        Debug.Log($"[SaveSystem] Saved slot {slotIndex}");
    }

    public static SaveData LoadGame(int slotIndex)
    {
        string key = "SaveSlot" + slotIndex;
        if (PlayerPrefs.HasKey(key))
        {
            string json = PlayerPrefs.GetString(key);
            return JsonUtility.FromJson<SaveData>(json);
        }
        return null;
    }

    public static void DeleteSlot(int slotIndex)
    {
        string key = "SaveSlot" + slotIndex;
        if (PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }
    }
}
