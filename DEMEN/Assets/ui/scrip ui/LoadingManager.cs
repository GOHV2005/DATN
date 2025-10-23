using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public RectTransform loadingIcon;  // cái hình tròn xoay
    public TMP_Text loadingText;       // chữ %

    public float minDisplayTime = 2f;  // hiện ít nhất 2 giây
    private float displayTimer = 0f;
    private float fakeProgress = 0f;

    void Start()
    {

        string sceneToLoad = SceneLoader.GetTargetScene();
        StartCoroutine(LoadAsync(sceneToLoad));
    }

    void Update()
    {
        if (loadingIcon != null)
            loadingIcon.Rotate(Vector3.forward * -200 * Time.deltaTime);
    }

    IEnumerator LoadAsync(string sceneName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        displayTimer = 0f;

        while (!op.isDone)
        {
            float realProgress = Mathf.Clamp01(op.progress / 0.9f);
            fakeProgress = Mathf.MoveTowards(fakeProgress, realProgress, Time.deltaTime * 0.5f);

            if (loadingText != null)
                loadingText.text = $"Đang tải... {(int)(fakeProgress * 100)}%";

            displayTimer += Time.deltaTime;

            if (fakeProgress >= 0.99f && displayTimer >= minDisplayTime)
            {
                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
