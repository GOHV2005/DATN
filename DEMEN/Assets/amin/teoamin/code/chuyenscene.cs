using UnityEngine;
using UnityEngine.SceneManagement;

public class chuyenscene : MonoBehaviour
{
    public string SceneName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SceneLoader.LoadScene(SceneName);
        }
    }
}
