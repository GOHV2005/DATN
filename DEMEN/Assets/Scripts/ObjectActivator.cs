using UnityEngine;

public class ObjectActivator : MonoBehaviour
{
    [SerializeField] string activatorTag = "Player";
    [SerializeField] bool deactivateOnExit = false;
    [SerializeField] GameObject[] objects = null;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(activatorTag)) return;

        foreach (var obj in objects)
        {
            if (obj != null && !obj.activeSelf)
                obj.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!deactivateOnExit || !collision.CompareTag(activatorTag)) return;

        foreach (var obj in objects)
        {
            if (obj != null && obj.activeSelf)
                obj.SetActive(false);
        }
    }
}
