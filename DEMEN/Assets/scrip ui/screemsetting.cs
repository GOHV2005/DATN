using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScreenSettings : MonoBehaviour
{
    [Header("UI")]
    public Toggle fullscreenToggle;        // Toggle để bật/tắt fullscreen
    public TMP_Dropdown resolutionDropdown; // Dùng TMP_Dropdown thay vì Dropdown

    private Resolution[] resolutions;

    void Start()
    {
        // Lấy tất cả độ phân giải hỗ trợ
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        // Tạo danh sách string để hiển thị
        var options = new System.Collections.Generic.List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height ;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height &&
                resolutions[i].refreshRate == Screen.currentResolution.refreshRate)
            {
                currentResolutionIndex = i;
            }
        }

        // Thêm vào Dropdown
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        // Gán sự kiện
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        fullscreenToggle.isOn = Screen.fullScreen;
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    // Đổi độ phân giải
    public void SetResolution(int resolutionIndex)
    {
        Resolution res = resolutions[resolutionIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen, res.refreshRate);
    }

    // Bật / tắt fullscreen
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
}
