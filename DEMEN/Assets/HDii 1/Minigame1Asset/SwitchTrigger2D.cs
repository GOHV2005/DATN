using UnityEngine;

public class SwitchTrigger2D : MonoBehaviour
{
    public GameObject pressFUIObject; // UI hiển thị "Nhấn F"

    private bool playerInRange = false;

    private void Start()
    {
        if (pressFUIObject != null)
            pressFUIObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            if (pressFUIObject != null)
                pressFUIObject.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            if (pressFUIObject != null)
                pressFUIObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            MinigameGateController gateController = FindObjectOfType<MinigameGateController>();
            if (gateController != null)
                gateController.ActivateSwitch();

            if (pressFUIObject != null)
                pressFUIObject.SetActive(false);
        }
    }
}
