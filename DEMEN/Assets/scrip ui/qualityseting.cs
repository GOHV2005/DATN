using UnityEngine;
using TMPro;

public class QualitySettingsManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Dropdown qualityDropdown; // Dropdown chọn chất lượng

    void Start()
    {
        // Xóa options cũ
        qualityDropdown.ClearOptions();

        // Lấy danh sách quality từ Unity (Project Settings → Quality)
        string[] qualityNames = QualitySettings.names;
        var options = new System.Collections.Generic.List<string>(qualityNames);

        // Thêm vào dropdown
        qualityDropdown.AddOptions(options);

        // Set giá trị ban đầu theo Quality hiện tại
        int currentQuality = QualitySettings.GetQualityLevel();
        qualityDropdown.value = currentQuality;
        qualityDropdown.RefreshShownValue();

        // Gán sự kiện
        qualityDropdown.onValueChanged.AddListener(SetQuality);
    }

    // Hàm đổi chất lượng
    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
        Debug.Log("Đổi chất lượng: " + QualitySettings.names[index]);
    }
}
