using UnityEngine;
using TMPro;

public class QualitySettingsManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Dropdown qualityDropdown;

    void Start()
    {
        // 🔥 LẤY SỐ LƯỢNG MỨC THỰC TẾ TỪ UNITY
        string[] actualQualityNames = QualitySettings.names;

        // (Tùy chọn) Dịch tên sang tiếng Việt — nhưng PHẢI CÓ ĐỦ SỐ LƯỢNG
        string[] vietnameseNames = GetVietnameseNames(actualQualityNames.Length);

        // Cập nhật dropdown
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(vietnameseNames));

        int current = QualitySettings.GetQualityLevel();
        current = Mathf.Clamp(current, 0, vietnameseNames.Length - 1);

        qualityDropdown.value = current;
        qualityDropdown.RefreshShownValue();

        qualityDropdown.onValueChanged.AddListener(SetQuality);
    }

    string[] GetVietnameseNames(int count)
    {
        // Đảm bảo luôn trả đủ `count` phần tử
        if (count <= 1) return new string[] { "Mặc Định" };
        if (count == 2) return new string[] { "Thấp", "Cao" };
        if (count == 3) return new string[] { "Thấp", "Trung Bình", "Cao" };
        if (count == 4) return new string[] { "Rất Thấp", "Thấp", "Trung Bình", "Cao" };
        if (count >= 5) return new string[] { "Rất Thấp", "Thấp", "Trung Bình", "Cao", "Rất Cao" };

        // Fallback
        string[] names = new string[count];
        for (int i = 0; i < count; i++)
            names[i] = "Mức " + (i + 1);
        return names;
    }

    public void SetQuality(int index)
    {
        // Kiểm tra an toàn
        if (index >= 0 && index < QualitySettings.names.Length)
        {
            QualitySettings.SetQualityLevel(index, true);
            Debug.Log($"Chất lượng đã đổi: {QualitySettings.names[index]} (index: {index})");
        }
        else
        {
            Debug.LogWarning($"Index {index} không hợp lệ. Tổng mức: {QualitySettings.names.Length}");
        }
    }
}