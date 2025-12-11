using UnityEngine;

public class MinigameTrigger2D : MonoBehaviour
{
    public string minigameName; // Minigame1 hoặc Minigame2
    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            SceneManagerHelper.Instance.GoToMinigame(minigameName);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Player vao trigger");
        if (collision.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playerInRange = false;
    }
}
