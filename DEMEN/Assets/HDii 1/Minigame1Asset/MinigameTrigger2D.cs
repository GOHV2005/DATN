using UnityEngine;

public class MiniggameTrigger2D : MonoBehaviour
{
    public string minigameName;

    private bool playerInRange = false;
    private bool isLoading = false;

    // STATIC -> không bị reset khi load lại scene
    private static bool hasPlayed = false;

    private Transform playerTransform;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (hasPlayed) return; // Không cho vào nếu đã chơi

            playerInRange = true;
            playerTransform = collision.transform;
            isLoading = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            playerTransform = null;
        }
    }

    void Update()
    {
        if (hasPlayed) return;  // Không cho chơi lại nếu đã chơi
        if (!playerInRange) return;
        if (isLoading) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            isLoading = true;
            hasPlayed = true;  // Đánh dấu đã chơi

            SceneManagerHelper.Instance.GoToMinigame(minigameName, playerTransform);
        }
    }
}
