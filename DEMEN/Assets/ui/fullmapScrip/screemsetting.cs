using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ScreenSettings : MonoBehaviour
{
    [Header("UI")]
    public Toggle fullscreenToggle;
    public TMP_Dropdown resolutionDropdown;

    // Danh sách các độ phân giải cơ bản bạn muốn hiển thị
    private static readonly (int width, int height)[] basicResolutions = {
        (1920, 1080), // Full HD
        (1600, 900),
        (1280, 720),  // HD
        (800, 600)
    };

    private List<Resolution> filteredResolutions = new List<Resolution>();

    void Start()
    {
        // Lấy tất cả độ phân giải hỗ trợ
        Resolution[] allResolutions = Screen.resolutions;

        // Chỉ giữ lại những độ phân giải có trong basicResolutions và thực sự được hệ thống hỗ trợ
        filteredResolutions.Clear();
        foreach (var basic in basicResolutions)
        {
            foreach (var res in allResolutions)
            {
                if (res.width == basic.width && res.height == basic.height)
                {
                    // Tránh trùng lặp (nhiều refresh rate có thể tạo cùng độ phân giải)
                    if (!filteredResolutions.Exists(r => r.width == res.width && r.height == res.height))
                    {
                        filteredResolutions.Add(res);
                    }
                    break; // tìm được rồi thì thôi, không cần lặp tiếp
                }
            }
        }

        // Nếu hệ thống không hỗ trợ 6 độ phân giải trên, fallback về danh sách hiện có
        if (filteredResolutions.Count == 0)
        {
            // fallback: lấy 6 độ phân giải cuối (thường là lớn nhất)
            int startIndex = Mathf.Max(0, allResolutions.Length - 6);
            for (int i = startIndex; i < allResolutions.Length; i++)
            {
                filteredResolutions.Add(allResolutions[i]);
            }
        }

        // Tạo options
        var options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < filteredResolutions.Count; i++)
        {
            string option = filteredResolutions[i].width + " x " + filteredResolutions[i].height;
            options.Add(option);

            if (filteredResolutions[i].width == Screen.currentResolution.width &&
                filteredResolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        fullscreenToggle.isOn = Screen.fullScreen;
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution res = filteredResolutions[resolutionIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode, res.refreshRateRatio);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
}