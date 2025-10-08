using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pauseMenuUI;      // Panel chính (Pause menu)
    public GameObject optionsMenuUI;    // Panel Options (Setting)

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        optionsMenuUI.SetActive(false);
        Time.timeScale = 0f; // Dừng game
        isPaused = true;
       
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
        Time.timeScale = 1f; // Chạy lại game
        isPaused = false;
      
        Cursor.visible = false;
    }

    public void OpenOptions()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f; // Đảm bảo game không bị dừng
        SceneManager.LoadScene("MainMenu"); // Thay tên scene của bạn ở đây
    }
}
