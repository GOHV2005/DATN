using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseSettingsMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject settingsMenuUI;   // Panel chính
    public GameObject optionsMenuUI;    // Panel options (âm thanh, quality)

    [Header("UI Controls")]
    public Slider volumeSlider;
    public Dropdown qualityDropdown;

    private bool isPaused = false;

    void Start()
    {
        // Tắt hết khi bắt đầu
        settingsMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);

        // Gán giá trị ban đầu
        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new System.Collections.Generic.List<string>() { "Low", "Medium", "High" });
            qualityDropdown.value = QualitySettings.GetQualityLevel();
            qualityDropdown.onValueChanged.AddListener(SetQuality);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsMenuUI.activeSelf)
            {
                BackFromOptions();
            }
            else
            {
                if (isPaused) Resume();
                else Pause();
            }
        }
    }

    public void Pause()
    {
        settingsMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Resume()
    {
        settingsMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void OpenOptions()
    {
        settingsMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);
    }

    public void BackFromOptions()
    {
        optionsMenuUI.SetActive(false);
        settingsMenuUI.SetActive(true);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // đổi tên Scene tùy bạn
    }

    // chỉnh âm thanh
    public void SetVolume(float value)
    {
        AudioListener.volume = value;
    }

    // chỉnh chất lượng
    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
    }
}
