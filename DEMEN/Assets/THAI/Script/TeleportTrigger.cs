using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportTrigger : MonoBehaviour
{
    public Animator animator; // Assign your animation controller
    public string nextSceneName; // Set this in Inspector
    private bool canTeleport = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            animator.SetTrigger("PlayAnimation"); // Trigger animation
            StartCoroutine(EnableTeleportAfterDelay());
            Debug.Log("Triggered by: " + other.name);
        }
    }

    private IEnumerator EnableTeleportAfterDelay()
    {
        yield return new WaitForSeconds(5f);
        canTeleport = true;
    }

    private void Update()
    {
        if (canTeleport && Input.GetKeyDown(KeyCode.E))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider2D>().bounds.size);
    }

}
