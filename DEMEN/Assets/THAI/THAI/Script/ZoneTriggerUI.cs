using UnityEngine;

public class ZoneTriggerUI : MonoBehaviour
{
    public BellGameManager gameManager; // drag your GameManager here

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.ShowGame();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.HideGame();
        }
    }
}
