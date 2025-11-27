using UnityEngine;

public class MinigameNPC : MonoBehaviour
{
    public string minigameSceneName = "Minigame2";

    private void OnMouseDown()
    {
        SceneManagerHelper.Instance.GoToMinigame(minigameSceneName);
    }
}