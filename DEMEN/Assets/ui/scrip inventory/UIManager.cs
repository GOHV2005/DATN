using UnityEngine;
using UnityEngine.Rendering;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject ButtonPanel;      // Panel chứa các nút
    public GameObject panelMap;
    public GameObject panelCraft;
    public GameObject panelInventory;

    
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

    // 👉 Dùng cho phím tắt (I, C, M) và cho cả nút bấm UI
    public void ShowInventory()
    {
        ShowPanel(panelInventory);
    }

    public void ShowCraft()
    {
        ShowPanel(panelCraft);
    }

    public void ShowMap()
    {
        ShowPanel(panelMap);
    }

    private void ShowPanel(GameObject targetPanel)
    {
        bool isActive = targetPanel.activeSelf;

        HideAll();

        if (!isActive) // nếu đang tắt thì bật lại
        {
            ButtonPanel.SetActive(true);
            targetPanel.SetActive(true);
        }
    }

    private void HideAll()
    {
        ButtonPanel.SetActive(false);
        panelMap.SetActive(false);
        panelCraft.SetActive(false);
        panelInventory.SetActive(false);
    }


}
