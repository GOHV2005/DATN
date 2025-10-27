using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DeathScreenUI : MonoBehaviour
{
    [Header("UI References")]
    public Image blackOverlay;
    public TextMeshProUGUI deathText;

    [Header("Settings")]
    public float fadeDuration = 2f;
    public float textDelay = 0.5f;  // Thời gian chờ trước khi hiện chữ
    public float textFadeDuration = 1.5f;

    private void Start()
    {
        // Ẩn lúc đầu
        blackOverlay.color = new Color(0, 0, 0, 0);
        deathText.color = new Color(deathText.color.r, deathText.color.g, deathText.color.b, 0);
        gameObject.SetActive(false);
    }

    public void ShowDeathScreen()
    {
        gameObject.SetActive(true);
        StartCoroutine(FadeInSequence());
    }

    private IEnumerator FadeInSequence()
    {
        // 1️⃣ Fade đen dần
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / fadeDuration);
            blackOverlay.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // 2️⃣ Đợi một chút rồi hiện chữ đỏ
        yield return new WaitForSeconds(textDelay);

        t = 0;
        while (t < textFadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / textFadeDuration);
            var c = deathText.color;
            deathText.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }

        // (Tuỳ chọn) chờ vài giây rồi restart game
        // yield return new WaitForSeconds(3f);
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
