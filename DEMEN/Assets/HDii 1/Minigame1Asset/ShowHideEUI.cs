using UnityEngine;

public class ShowHideEUI : MonoBehaviour
{
    public GameObject pressEUIObject; // UI nút E
    public string playerTag = "Player"; // Tag của Player, mặc định "Player"

    private void Start()
    {
        if (pressEUIObject != null)
            pressEUIObject.SetActive(false); // Ẩn UI mặc định
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(playerTag))
        {
            if (pressEUIObject != null)
                pressEUIObject.SetActive(true); // Hiện nút E khi player lại gần
            Debug.Log("Player đã vào vùng trigger");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(playerTag))
        {
            if (pressEUIObject != null)
                pressEUIObject.SetActive(false); // Ẩn nút E khi player đi xa
        }
    }
}
