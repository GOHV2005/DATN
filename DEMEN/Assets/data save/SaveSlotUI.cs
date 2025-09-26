using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class SaveSlotUI : MonoBehaviour
{
    public TMP_Text sceneNameText;
    public TMP_Text playTimeText;
    public int slotIndex;
    public SaveMenuManager menuManager; // gắn từ Inspector

    private SaveData data;
    private bool isSelected = false;

    void Start()
    {
        LoadSlot();
        GetComponent<Image>().color = Color.white;
    }

    public void LoadSlot()
    {
        if (sceneNameText == null || playTimeText == null)
        {
            Debug.LogError($"[SaveSlotUI] TMP_Text chưa được gán ở slot {slotIndex}");
            return;
        }

        data = SaveSystem.LoadGame(slotIndex);

        if (data == null)
        {
            sceneNameText.text = "Chơi Mới";
            playTimeText.text = "";
        }
        else
        {
            sceneNameText.text = string.IsNullOrEmpty(data.sceneName) ? "Unknown" : data.sceneName;
            TimeSpan time = TimeSpan.FromSeconds(data.playTime);
            playTimeText.text = $"{time.Hours}h {time.Minutes}m";
        }
    }


    public void OnClickSlot()
    {
        if (!isSelected)
        {
            // báo cho menu manager biết slot này được chọn
            if (menuManager != null) menuManager.SelectSlot(this);

            // chọn slot
            isSelected = true;
            GetComponent<Image>().color = new Color(0.8f, 0.8f, 1f, 1f);

        }
        else
        {
            // click lần 2 -> load game
            if (data == null)
            {
                SaveData newData = new SaveData
                {
                    sceneName = "StartScenetest",
                    playTime = 0f
                };
                SaveSystem.SaveGame(newData, slotIndex);
                SceneManager.LoadScene(newData.sceneName);
            }
            else
            {
                SceneManager.LoadScene(data.sceneName);
            }
        }
    }

    public void Deselect()
    {
        isSelected = false;
        GetComponent<Image>().color = Color.white;
    }

    public int GetSlotIndex()
    {
        return slotIndex;
    }
}
