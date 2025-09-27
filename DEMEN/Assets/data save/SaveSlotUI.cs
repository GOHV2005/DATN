using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class SaveSlotUI : MonoBehaviour
{
    public TMP_Text sceneNameText;
    public TMP_Text playTimeText;
    public int slotIndex;
    public SaveMenuManager menuManager;

    private SaveData data;

    void Start()
    {
        LoadSlot();
    }

    public void LoadSlot()
    {
        data = SaveSystem.LoadGame(slotIndex);

        if (data == null)
        {
            sceneNameText.text = "Chơi Mới";
            playTimeText.text = ""; // Chưa có thời gian → không hiện
        }
        else
        {
            sceneNameText.text = string.IsNullOrEmpty(data.GetSceneName()) ? "Unknown" : data.GetSceneName();
            TimeSpan time = TimeSpan.FromSeconds(data.GetPlayTime());
            playTimeText.text = $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
        }
    }

    public void OnClickSlot()
    {
        if (menuManager == null) return;

        // Nếu chưa chọn slot này → chọn slot
        if (menuManager.GetSelectedSlot() != this)
        {
            menuManager.SelectSlot(this);
        }
        else
        {
            // Slot đã được chọn → load game
            PlayerPrefs.SetInt("CurrentSlot", slotIndex);
            string sceneToLoad = (data != null && !string.IsNullOrEmpty(data.GetSceneName()))
                                  ? data.GetSceneName()
                                  : "StartScenetest";
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    public int GetSlotIndex() => slotIndex;

    public bool HasData() => data != null;
}
