using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTriggered : MonoBehaviour
{
    public string sceneName; // gán scene cần chuyển
    private void OnTriggerEnter2D(Collider2D collision)
    {
        AutoSaveRAM.Instance?.Capture();
        SceneLoader.LoadScene(sceneName);
    }
    
}
