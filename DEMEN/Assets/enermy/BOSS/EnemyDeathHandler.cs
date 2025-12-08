// EnemyDeathHandler.cs
using UnityEngine;
using System;

/// <summary>
/// Xử lý hành vi "khi enemy chết" và thông báo cho hệ thống quản lý wave.
/// Gắn script này vào prefab Enemy.
/// </summary>
public class EnemyDeathHandler : MonoBehaviour
{
    // Event static: bất kỳ nơi nào cũng có thể đăng ký để nghe khi enemy chết
    public static event Action OnEnemyDied;

    /// <summary>
    /// Gọi hàm này thay vì Destroy(gameObject) trực tiếp trong các script khác (ví dụ: EnemyHealth).
    /// </summary>
    public void Die()
    {
        OnEnemyDied?.Invoke(); // Gửi tín hiệu "có 1 enemy vừa chết"
        Destroy(gameObject);
    }
}