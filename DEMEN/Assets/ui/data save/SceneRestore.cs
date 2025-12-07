using UnityEngine;

public class SceneRestore : MonoBehaviour
{
    void Start()
    {
        int slot = PlayerPrefs.GetInt("CurrentSlot", 0);
        SaveData data = SaveSystem.LoadGame(slot);
        if (data == null) return;

        foreach (var obj in FindObjectsOfType<SaveableObject>())
        {
            if (!data.existingObjects.Contains(obj.guid))
            {
                Destroy(obj.gameObject);
            }
        }
    }
}
