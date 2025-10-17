using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("UI")]
    public Slider healthSlider;   // Slider hiển thị máu

    [Header("Health Values")]
    public int maxHealth = 100;
    [Range(0, 100)]               // Cho phép chỉnh trực tiếp trong Inspector
    public int currentHealth = 100;

    private void Start()
    {
        // Gán giá trị max cho slider
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }

    private void Update()
    {
        // Cho phép chỉnh trực tiếp currentHealth trong Inspector
        // và slider sẽ cập nhật theo
        healthSlider.value = currentHealth;
    }

    public void RestoreHealth(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        healthSlider.value = currentHealth;
    }
}
