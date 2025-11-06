using UnityEngine;
using TMPro;

public class QualitySettingsManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Dropdown qualityDropdown;

    private readonly string[] vietnameseNames = { "Thấp", "Trung Bình", "Cao" }; // 3 mức

    void Start()
    {
        // Luôn dùng đúng số lượng names đã định nghĩa
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(vietnameseNames));

        // Lấy current level, nhưng giới hạn trong [0, 2]
        int current = QualitySettings.GetQualityLevel();
        current = Mathf.Clamp(current, 0, vietnameseNames.Length - 1);

        // ⚠️ RẤT QUAN TRỌNG: Đặt lại quality level nếu nó vượt quá
        QualitySettings.SetQualityLevel(current, true);

        // Gán giá trị an toàn
        qualityDropdown.value = current;
        qualityDropdown.RefreshShownValue();

        qualityDropdown.onValueChanged.AddListener(SetQuality);
    }

    public void SetQuality(int index)
    {
        // Phòng thủ: chỉ xử lý nếu index hợp lệ
        if (index >= 0 && index < vietnameseNames.Length)
        {
            QualitySettings.SetQualityLevel(index, true);
            Debug.Log("Chất lượng đã đổi: " + vietnameseNames[index]);
        }
        else
        {
            // Nếu vẫn còn index lỗi → reset về mặc định
            Debug.LogWarning("Index không hợp lệ. Đặt lại về 'Trung Bình'.");
            qualityDropdown.value = 1; // Trung Bình
            QualitySettings.SetQualityLevel(1, true);
        }
    }
}