using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject ButtonPanel;
    public GameObject panelInventory;
    public GameObject panelMap;
    public GameObject panelHealth; // Slider máu

    public static UIManager Instance;
    public static bool IsUIOpen = false;

    private void Awake()
    {        // Khởi đầu tắt tất cả
        HideAll();
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // giữ UI xuyên scene
        }
        else
        {
            Destroy(gameObject);
            return;
        }


    }

    private void Update()
    {
        // Kiểm tra Player tồn tại
        bool hasPlayer = GameObject.FindWithTag("Player") != null;

        // Chỉ bật Health nếu có Player
        if (panelHealth != null)
            panelHealth.SetActive(hasPlayer);

        // Chỉ cho phép thao tác Inventory / Craft / Map nếu Player tồn tại
        if (!hasPlayer) return;

        if (Input.GetKeyDown(KeyCode.I))
            ShowPanel(panelInventory);
        else if (Input.GetKeyDown(KeyCode.M))
            ShowPanel(panelMap);
        else if (Input.GetKeyDown(KeyCode.Escape))
            HideAll();
    }

    public void ShowInventory() => ShowPanel(panelInventory);
    public void ShowMap() => ShowPanel(panelMap);

    private void ShowPanel(GameObject targetPanel)
    {
        bool isActive = targetPanel.activeSelf;

        HideAll();

        if (!isActive)
        {
            ButtonPanel.SetActive(true);
            targetPanel.SetActive(true);

            IsUIOpen = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void HideAll()
    {
        if (ButtonPanel != null) ButtonPanel.SetActive(false);
        if (panelInventory != null) panelInventory.SetActive(false);
        if (panelMap != null) panelMap.SetActive(false);

        IsUIOpen = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
