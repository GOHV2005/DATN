using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class FlyItemToInventory : MonoBehaviour
{
    public float flyTime = 0.5f;

    private RectTransform rect;
    private Image img;
    private Action onArrive;

    public void Init(Sprite sprite, Vector3 startScreenPos, RectTransform target, Action onArriveCallback)
    {
        rect = gameObject.AddComponent<RectTransform>();
        img = gameObject.AddComponent<Image>();

        img.sprite = sprite;
        img.raycastTarget = false;

        rect.sizeDelta = new Vector2(48, 48);
        rect.position = startScreenPos;

        onArrive = onArriveCallback;

        StartCoroutine(Fly(target));
    }

    IEnumerator Fly(RectTransform target)
    {
        Vector3 start = rect.position;
        Vector3 end = target.position;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / flyTime;
            rect.position = Vector3.Lerp(start, end, t);
            rect.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.3f, t);
            yield return null;
        }

        onArrive?.Invoke();
        Destroy(gameObject);
    }
}
