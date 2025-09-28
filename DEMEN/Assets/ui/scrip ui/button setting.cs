using UnityEngine;

public class MenuController : MonoBehaviour
{
    [Header("Panels")]
    public RectTransform startPanel;   // Panel Start
    public RectTransform settingPanel; // Panel Setting
    public RectTransform SavePanel;

    [Header("Animation Settings")]
    public float duration = 0.5f;   // thời gian chạy animation
    public float offsetX = 1920f;   // khoảng cách dịch chuyển ngang (theo pixel)

    // Nút Start → Setting
    public void OpenSaveMenu()
    {
        StopAllCoroutines();
        StartCoroutine(MovePanel(startPanel, startPanel.anchoredPosition, new Vector2(-offsetX, 0), duration));
        StartCoroutine(MovePanel(SavePanel, SavePanel.anchoredPosition, Vector2.zero, duration));
    }
    // Nút Option → Setting
    public void OpenSetting()
    {
        StopAllCoroutines();
        StartCoroutine(MovePanel(startPanel, startPanel.anchoredPosition, new Vector2(-offsetX, 0), duration));
        StartCoroutine(MovePanel(settingPanel, settingPanel.anchoredPosition, Vector2.zero, duration));
    }

    // Nút Exit → cũng giống BackToStart (Start panel vào giữa, Setting panel ra phải)
    public void Exit()
    {
        StopAllCoroutines();
        StartCoroutine(MovePanel(startPanel, startPanel.anchoredPosition, Vector2.zero, duration));
        StartCoroutine(MovePanel(settingPanel, settingPanel.anchoredPosition, new Vector2(offsetX, 0), duration));
        StartCoroutine(MovePanel(SavePanel, SavePanel.anchoredPosition, new Vector2(offsetX, 0), duration));
    }


    // Hàm chạy animation di chuyển panel
    private System.Collections.IEnumerator MovePanel(RectTransform panel, Vector2 from, Vector2 to, float time)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            panel.anchoredPosition = Vector2.Lerp(from, to, elapsed / time);
            elapsed += Time.deltaTime;
            yield return null;
        }
        panel.anchoredPosition = to;
    }
}
