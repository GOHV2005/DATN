using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject panelInventory;

    public static UIManager Instance;
    public static bool IsUIOpen = false;

    private void Awake()
    {
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
        // Chỉ cho phép mở UI nếu có Player trong scene
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
            // Đang mở → đóng
            HideAll();
            Time.timeScale = 1f;
        }
        else
        {
            Time.timeScale = 0f;
            // Đang đóng → mở panel này
            HideAll();
            targetPanel.SetActive(true);
            IsUIOpen = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void HideAll()
    {
        if (panelInventory != null) panelInventory.SetActive(false);

        IsUIOpen = false;
        // Optional: Ẩn con trỏ khi đóng UI (nếu bạn dùng FPS/TPS)
        // Cursor.visible = false;
        // Cursor.lockState = CursorLockMode.Locked;
    }
}