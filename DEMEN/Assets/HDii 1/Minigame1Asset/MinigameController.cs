using UnityEngine;

public class MinigameController : MonoBehaviour
{
    public GameObject minigamePanel;
    public GameObject switchObject;

    [Header("Player Controller (script của teammate)")]
    public MonoBehaviour playerController;

    void Start()
    {
        minigamePanel.SetActive(false);
        switchObject.SetActive(false);
    }

    public void StartMinigame()
    {
        if (playerController != null)
            playerController.enabled = false;

        minigamePanel.SetActive(true);
    }

    public void EndMinigame()
    {
        minigamePanel.SetActive(false);

        if (playerController != null)
            playerController.enabled = true;

        switchObject.SetActive(true);
    }
}
