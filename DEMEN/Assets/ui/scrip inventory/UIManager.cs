using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject ButtonPanel;
    public GameObject panelMap;
    public GameObject panelCraft;
    public GameObject panelInventory;

    public static bool IsUIOpen = false;

    void Start()
    {
        HideAll();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ShowPanel(panelInventory);
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            ShowPanel(panelCraft);
        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            ShowPanel(panelMap);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            HideAll();
        }
    }

    public void ShowInventory() => ShowPanel(panelInventory);
    public void ShowCraft() => ShowPanel(panelCraft);
    public void ShowMap() => ShowPanel(panelMap);

    private void ShowPanel(GameObject targetPanel)
    {
        bool isActive = targetPanel.activeSelf;

        HideAll();

        if (!isActive)
        {
            ButtonPanel.SetActive(true);
            targetPanel.SetActive(true);

            IsUIOpen = true;   // 🔒 bật UI → khóa player
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void HideAll()
    {
        ButtonPanel.SetActive(false);
        panelMap.SetActive(false);
        panelCraft.SetActive(false);
        panelInventory.SetActive(false);

        IsUIOpen = false;   // 🔓 tắt UI → mở player
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
