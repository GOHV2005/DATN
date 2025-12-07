using UnityEngine;

public class FogZone : MonoBehaviour
{
    public float fadeDuration = 1.5f; // seconds
    private SpriteRenderer fogRenderer;
    private bool isFading = false;

    void Start()
    {
        fogRenderer = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isFading)
        {
            StartCoroutine(FadeOut());
        }
    }

    private System.Collections.IEnumerator FadeOut()
    {
        isFading = true;
        Color startColor = fogRenderer.color;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / fadeDuration);
            fogRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        fogRenderer.enabled = false; // hide completely
    }
}
