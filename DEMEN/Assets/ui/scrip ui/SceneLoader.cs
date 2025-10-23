using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    private static string targetScene;

    // Gọi cái này khi muốn đổi scene
    public static void LoadScene(string sceneName)
    {
        targetScene = sceneName;
        SceneManager.LoadScene("LoadingScene"); // luôn đi qua LoadingScene
    }

    // LoadingManager trong LoadingScene gọi hàm này
    public static string GetTargetScene()
    {
        return targetScene;
    }
}
