// FullMapUI.cs
using UnityEngine;

public class FullMapUI : MonoBehaviour
{
    public GameObject panelFullMap;
    public FullMapController fullMapController;

    void Start()
    {
        if (panelFullMap != null)
            panelFullMap.SetActive(false);
    }

    void Update()
    {
        if (GameObject.FindWithTag("Player") == null) return;

        if (Input.GetKeyDown(KeyCode.M))
        {
            bool willOpen = !panelFullMap.activeSelf;
            panelFullMap.SetActive(willOpen);
            Time.timeScale = willOpen ? 0f : 1f;
            if (willOpen && fullMapController != null)
            {
                fullMapController.ResetView();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && panelFullMap.activeSelf)
        {
            panelFullMap.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    // Gọi từ nút UI nếu cần
    public void OpenFullMap()
    {
        panelFullMap.SetActive(true);
        Time.timeScale = 0f;
        if (fullMapController != null)
            fullMapController.ResetView();
    }

    public void CloseFullMap()
    {
        panelFullMap.SetActive(false);
        Time.timeScale = 1f;
    }
}