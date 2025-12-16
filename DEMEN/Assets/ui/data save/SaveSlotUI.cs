using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    public TMP_Text slotInfoText; // <-- Dùng 1 TMP_Text duy nhất
    public int slotIndex;
    public SaveMenuManager menuManager;

    private SaveData data;
    private bool isSelected = false;

    void Start()
    {
        LoadSlot();
    }

    public void LoadSlot()
    {
        data = SaveSystem.LoadGame(slotIndex);

        if (data == null || data.scenes.Count == 0)
        {
            slotInfoText.text = "Chơi Mới";
        }
        else
        {
            var lastScene = data.scenes[data.scenes.Count - 1];
            string sceneName = string.IsNullOrEmpty(lastScene.sceneName) ? "Unknown" : lastScene.sceneName;
            string playTime = lastScene.GetPlayTimeString();
            slotInfoText.text = $"{sceneName}\n{playTime}";
        }
    }

    public void OnClickSlot()
    {
        if (!isSelected)
        {
            menuManager?.SelectSlot(this);
            isSelected = true;
        }
        else
        {
            PlayerPrefs.SetInt("CurrentSlot", slotIndex);

            // Nếu chưa có save data → tạo mới (rỗng nhưng hợp lệ)
            if (data == null || data.scenes.Count == 0)
            {
                SaveData newData = new SaveData();

                // 👇 TẠO MỘT CHECKPOINT "RỖNG" HOẶC DỰ PHÒNG
                // Ví dụ: dùng scene mặc định + vị trí (0,0,0)
                // Hoặc bạn có thể lưu scene hiện tại nếu đang trong gameplay
                SceneSaveData defaultCheckpoint = new SceneSaveData
                {
                    sceneName = "a", // hoặc "MainMenu"? tuỳ bạn
                    position = Vector3.zero,
                    playTime = 0f
                };
                newData.AddScene(defaultCheckpoint);

                // Lưu vào hệ thống
                SaveSystem.SaveGame(slotIndex, newData);

                // Cập nhật lại data local để load đúng
                data = newData;

                Debug.Log($"[SAVE SLOT] Created new save file for Slot {slotIndex}");
            }

            // Luôn load từ data (đã có hoặc mới tạo)
            string sceneName = data.scenes[data.scenes.Count - 1].sceneName;
            SceneLoader.LoadScene(sceneName);
        }
    }

    public void Deselect() => isSelected = false;
    public int GetSlotIndex() => slotIndex;
}