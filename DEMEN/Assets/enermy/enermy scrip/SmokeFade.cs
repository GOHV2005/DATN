using System.Collections;
using UnityEngine;

public class SmokeFade : MonoBehaviour
{
    public float lifeTime = 1.2f;      // tổng thời gian tồn tại
    public float fadeTime = 0.6f;      // thời gian mờ dần
    public Vector3 drift = new Vector3(0, 0.5f, 0); // bay nhẹ lên

    private SpriteRenderer sr;
    private Color startColor;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        startColor = sr.color;
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        float t = 0f;

        // chờ 1 đoạn trước khi fade
        yield return new WaitForSeconds(lifeTime - fadeTime);

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / fadeTime);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, a);
            transform.position += drift * Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
