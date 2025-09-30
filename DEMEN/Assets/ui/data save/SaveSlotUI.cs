using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    public TMP_Text sceneNameText;
    public TMP_Text playTimeText;
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
            sceneNameText.text = "Chơi Mới";
            playTimeText.text = "";
        }
        else
        {
            // Lấy scene cuối cùng đã lưu
            var lastScene = data.scenes[data.scenes.Count - 1];
            sceneNameText.text = string.IsNullOrEmpty(lastScene.sceneName) ? "Unknown" : lastScene.sceneName;
            playTimeText.text = lastScene.GetPlayTimeString();
        }
    }

    public void OnClickSlot()
    {
        if (!isSelected)
        {
            if (menuManager != null) menuManager.SelectSlot(this);
            isSelected = true; // chọn lần đầu
        }
        else
        {
            // Slot đã chọn → vào game
            PlayerPrefs.SetInt("CurrentSlot", slotIndex);
            string sceneName = "StartScenetest";

            if (data != null && data.scenes.Count > 0)
                sceneName = data.scenes[data.scenes.Count - 1].sceneName;

            // Dùng SceneLoader thay vì load trực tiếp
            SceneLoader.LoadScene(sceneName);
        }
    }


    public void Deselect()
    {
        isSelected = false;
    }

    public int GetSlotIndex() => slotIndex;
}
