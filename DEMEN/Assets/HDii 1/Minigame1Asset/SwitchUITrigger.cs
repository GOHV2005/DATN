using UnityEngine;

public class SwitchUITrigger : MonoBehaviour
{
    public GameObject pressFUIObject; // UI hiển thị "Nhấn F"

    private bool playerInRange = false;

    private void Start()
    {
        if (pressFUIObject != null)
            pressFUIObject.SetActive(false); // ẩn UI lúc đầu
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            if (pressFUIObject != null)
                pressFUIObject.SetActive(true); // hiện UI khi player vào vùng
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            if (pressFUIObject != null)
                pressFUIObject.SetActive(false); // ẩn UI khi player rời vùng
        }
    }
}
