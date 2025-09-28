using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    [Header("Audio")]
    public AudioMixer audioMixer;   // Kéo AudioMixer vào đây
    public Slider volumeSlider;     // Kéo Slider vào đây

    private void Start()
    {
        // Lấy giá trị volume đã lưu (nếu có)
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        volumeSlider.value = savedVolume;
        SetVolume(savedVolume);

        // Gán sự kiện khi kéo slider
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    // Hàm chỉnh volume
    public void SetVolume(float value)
    {
        // value (0 → 1), đổi sang dB (-80 → 0)
        float volume = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat("MasterVolume", volume);

        // Lưu lại khi thoát game
        PlayerPrefs.SetFloat("MasterVolume", value);
    }
}
