using UnityEngine;
using UnityEngine.UI;

public class BossUIManager : MonoBehaviour
{
    public static BossUIManager Instance;

    [SerializeField] private Slider bossHealthSlider;

    private Health currentBossHealth;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        bossHealthSlider.gameObject.SetActive(false);
    }

    public void Show(Health bossHealth)
    {
        if (bossHealth == null) return;

        currentBossHealth = bossHealth;

        bossHealthSlider.maxValue = bossHealth.maxHealth;
        bossHealthSlider.value = bossHealth.currentHealth;
        bossHealthSlider.gameObject.SetActive(true);

        bossHealth.onHealthChanged += UpdateHP;
        bossHealth.onDeath += Hide;
    }

    public void Hide()
    {
        bossHealthSlider.gameObject.SetActive(false);

        if (currentBossHealth != null)
        {
            currentBossHealth.onHealthChanged -= UpdateHP;
            currentBossHealth.onDeath -= Hide;
            currentBossHealth = null;
        }
    }

    public void UpdateHP(float hp)
    {
        bossHealthSlider.value = hp;
    }
}
