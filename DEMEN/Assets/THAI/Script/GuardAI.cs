using UnityEngine;

public class GuardAI : MonoBehaviour
{
    public Transform player;
    public float speed = 3f;

    void Update()
    {
        if (player != null)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                speed * Time.deltaTime
            );
        }
    }
}
