// EnemyController.cs
using UnityEngine;
using System;

public class EnemyController : MonoBehaviour
{
    public static event Action OnEnemyKilled; // (Cách 1: dùng static event – đơn giản)

    // Hoặc nếu bạn muốn linh hoạt hơn (nhiều spawner), dùng cách 2 bên dưới

    // Gọi hàm này trong logic "chết" của bạn
    public void NotifyDeath()
    {
        OnEnemyKilled?.Invoke();
        Destroy(gameObject);
    }
}