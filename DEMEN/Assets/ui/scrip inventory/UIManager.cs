using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject panelInventory;
    public static bool IsTalkingToNPC = false;
    public static UIManager Instance;
    public static bool IsUIOpen = false;
    private void Awake()
    {
        IsTalkingToNPC = false;
        HideAll();
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Update()
    {
        if (GameObject.FindWithTag("Player") == null)
            return;

        if (Input.GetKeyDown(KeyCode.I))
            TogglePanel(panelInventory);
        else if (Input.GetKeyDown(KeyCode.Escape))
            HideAll();
    }

    public void ShowInventory() => TogglePanel(panelInventory);

    private void TogglePanel(GameObject targetPanel)
    {
        if (targetPanel.activeSelf)
        {
            HideAll();
            Time.timeScale = 1f;
        }
        else
        {
            Time.timeScale = 0f;
            HideAll();
            targetPanel.SetActive(true);
            IsUIOpen = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void HideAll()
    {
        if (panelInventory != null) panelInventory.SetActive(false);
        IsUIOpen = false;
    }

    // ================ INPUT CONTROL ================
    private static PauseMenu _pauseMenu;
    private static bool _pauseMenuChecked = false;

    private static PauseMenu GetPauseMenu()
    {
        if (!_pauseMenuChecked)
        {
            var pauseMenus = Object.FindObjectsByType<PauseMenu>(FindObjectsSortMode.None);
            _pauseMenu = pauseMenus.Length > 0 ? pauseMenus[0] : null;
            _pauseMenuChecked = true;
        }
        return _pauseMenu;
    }

    public static bool IsGameplayInputAllowed
    {
        get
        {
            bool inventoryOpen = Instance != null && Instance.panelInventory != null && Instance.panelInventory.activeSelf;

            var pauseMenu = GetPauseMenu();
            bool pauseOpen = pauseMenu != null &&
                            ((pauseMenu.pauseMenuUI != null && pauseMenu.pauseMenuUI.activeSelf) ||
                             (pauseMenu.optionsMenuUI != null && pauseMenu.optionsMenuUI.activeSelf));

            return !inventoryOpen && !pauseOpen && !IsTalkingToNPC;
        }
    }
}