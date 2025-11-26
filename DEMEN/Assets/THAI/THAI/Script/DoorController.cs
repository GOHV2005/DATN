using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Animator animator;
    public Collider2D doorCollider;

    private bool isOpen = false;

    public void OpenDoor()
    {
        if (!isOpen)
        {
            isOpen = true;

            // Play animation
            if (animator != null)
            {
                animator.SetTrigger("Open");
            }

            // Disable collider so player can pass
            if (doorCollider != null)
            {
                doorCollider.enabled = false;
            }
        }
    }
}
