using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneRestore : MonoBehaviour
{
    void Start()
    {
        int slot = PlayerPrefs.GetInt("CurrentSlot", 0);
        SaveData data = SaveSystem.LoadGame(slot);
        if (data == null) return;

        string currentScene = SceneManager.GetActiveScene().name;

        // Kiểm tra scene này đã có trong save data chưa
        bool sceneExistsInSave = data.scenes.Exists(s => s.sceneName == currentScene);

        foreach (var obj in FindObjectsOfType<SaveableObject>())
        {
            // Chỉ áp dụng cho scene hiện tại
            if (obj.sceneName != currentScene) continue;

            // Tìm bản lưu của object này
            var savedObj = data.saveableObjects.Find(o => o.guid == obj.guid && o.sceneName == currentScene);

            if (savedObj != null)
            {
                // Khôi phục trạng thái nếu có ISaveable
                var saveableComp = obj.GetComponent<SaveData.ISaveable>();
                saveableComp?.RestoreState(savedObj.savedState);
            }
            else if (!sceneExistsInSave)
            {
                // Nếu scene chưa có bản lưu → tạo mới cho object
                SaveData.SaveableObjectRef newSave = new SaveData.SaveableObjectRef(
                    obj.guid,
                    obj.sceneName,
                    obj.GetComponent<SaveData.ISaveable>()?.CaptureState()
                );
                data.saveableObjects.Add(newSave);
            }
            else
            {
                // Scene đã có trong save nhưng object không tồn tại trong save → xóa
                Debug.Log($"[SceneRestore] Destroying object '{obj.name}' (guid={obj.guid}) not found in save data.");
                Destroy(obj.gameObject);
            }
        }

        // Chỉ lưu file nếu scene mới được thêm object
        if (!sceneExistsInSave)
        {
            SaveSystem.SaveGame(slot, data);
            Debug.Log($"[SceneRestore] Scene '{currentScene}' objects auto-saved for first time.");
        }

        Debug.Log($"[SceneRestore] Scene '{currentScene}' restored. Scene exists in save: {sceneExistsInSave}");
    }
}
