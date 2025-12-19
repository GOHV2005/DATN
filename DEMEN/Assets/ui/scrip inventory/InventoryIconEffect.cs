using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InventoryIconEffect : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite closedSprite;
    public Sprite openSprite;

    [Header("Scale Effect")]
    public float openScale = 1.2f;
    public float scaleTime = 0.12f;

    private Image img;
    private Vector3 originalScale;
    private Coroutine effectRoutine;

    private void Awake()
    {
        img = GetComponent<Image>();
        originalScale = transform.localScale;
        img.sprite = closedSprite;
    }

    public void PlayOpenEffect()
    {
        if (effectRoutine != null)
            StopCoroutine(effectRoutine);

        effectRoutine = StartCoroutine(OpenRoutine());
    }

    public void PlayCloseEffect()
    {
        if (effectRoutine != null)
            StopCoroutine(effectRoutine);

        effectRoutine = StartCoroutine(CloseRoutine());
    }

    IEnumerator OpenRoutine()
    {
        img.sprite = openSprite;
        yield return ScaleTo(originalScale * openScale);
    }

    IEnumerator CloseRoutine()
    {
        yield return ScaleTo(originalScale);
        img.sprite = closedSprite;
    }

    IEnumerator ScaleTo(Vector3 target)
    {
        Vector3 start = transform.localScale;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / scaleTime;
            transform.localScale = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.localScale = target;
    }
}
