using UnityEngine;
using TMPro;

public class QualitySettingsManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Dropdown qualityDropdown; // Dropdown chọn chất lượng

    // Bảng dịch Anh → Việt
    private readonly string[] vietnameseNames = { "Rất Thấp", "Thấp", "Trung Bình", "Cao", "Rất Cao" };

    void Start()
    {
        // Xóa options cũ
        qualityDropdown.ClearOptions();

        // Lấy danh sách quality gốc
        string[] qualityNames = QualitySettings.names;


        // Nếu số lượng khớp, thay bằng tên tiếng Việt
        var options = new System.Collections.Generic.List<string>();
        for (int i = 0; i < qualityNames.Length && i < vietnameseNames.Length; i++)
        {
            options.Add(vietnameseNames[i]);
        }

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
        Debug.Log("Đổi chất lượng: " + vietnameseNames[index]);
    }
}
