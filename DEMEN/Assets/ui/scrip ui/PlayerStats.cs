using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    public float maxStamina = 100f;
    public float currentStamina;

    [Header("UI - Health")]
    public Image healthFill;           // Thanh máu thật
    public Image healthDamageDelay;    // Thanh máu đệm

    [Header("UI - Stamina")]
    public Image staminaFill;          // Thanh stamina thật
    public Image staminaDelay;         // Thanh stamina đệm

    public float smoothSpeed = 2f;     // tốc độ tụt mượt

    void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
    }

    void Update()
    {
        // Test: mất máu khi nhấn H
        if (Input.GetKeyDown(KeyCode.H))
            TakeDamage(20);

        // Test stamina: giữ Space để mất năng lượng
        if (Input.GetKey(KeyCode.Space))
            UseStamina(20 * Time.deltaTime);
        else
            RecoverStamina(10 * Time.deltaTime);

        UpdateUI();
    }

    // ----- Health -----
    void TakeDamage(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
    }

    // ----- Stamina -----
    void UseStamina(float amount)
    {
        currentStamina = Mathf.Clamp(currentStamina - amount, 0, maxStamina);
    }

    void RecoverStamina(float amount)
    {
        currentStamina = Mathf.Clamp(currentStamina + amount, 0, maxStamina);
    }

    // ----- UI Update -----
    void UpdateUI()
    {
        float healthPercent = currentHealth / maxHealth;
        float staminaPercent = currentStamina / maxStamina;

        // Máu thật
        healthFill.fillAmount = healthPercent;
        // Máu đệm (tụt mượt)
        if (healthDamageDelay.fillAmount > healthPercent)
            healthDamageDelay.fillAmount = Mathf.Lerp(
                healthDamageDelay.fillAmount,
                healthPercent,
                Time.deltaTime * smoothSpeed
            );
        else
            healthDamageDelay.fillAmount = healthPercent;

        // Stamina thật
        staminaFill.fillAmount = staminaPercent;
        // Stamina đệm (tụt mượt)
        if (staminaDelay.fillAmount > staminaPercent)
            staminaDelay.fillAmount = Mathf.Lerp(
                staminaDelay.fillAmount,
                staminaPercent,
                Time.deltaTime * smoothSpeed
            );
        else
            staminaDelay.fillAmount = staminaPercent;
    }
}
