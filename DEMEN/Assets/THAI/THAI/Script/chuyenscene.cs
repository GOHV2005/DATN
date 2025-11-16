using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneActivator : MonoBehaviour
{
    public string sceneName;

    void OnEnable()
    {

        if (!string.IsNullOrEmpty(sceneName))
        {
            Debug.Log($"Scene Activator: Kích hoạt chuyển đến scene: {sceneName}");
            // Thực hiện lệnh chuyển scene
            SceneManager.LoadScene(sceneName);
        }

    }
}