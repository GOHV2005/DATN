// EnemyCore.cs
using UnityEngine;

public class EnemyCore : MonoBehaviour
{
    public void Die()
    {
        // Gọi trực tiếp Action (vì giờ nó là public)
        EnemyDeathHandler.OnEnemyDied?.Invoke();
        Destroy(gameObject);
    }
}