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
            string sceneName = "CutScence";

            if (data != null && data.scenes.Count > 0)
                sceneName = data.scenes[data.scenes.Count - 1].sceneName;

            SceneLoader.LoadScene(sceneName);
        }
    }

    public void Deselect() => isSelected = false;
    public int GetSlotIndex() => slotIndex;
}