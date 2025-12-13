using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class SaveableObject : MonoBehaviour
{
    [SerializeField] public string guid;
    [SerializeField] public string sceneName;  // 👈 MỚI - mỗi object biết mình thuộc scene nào

    private void Reset()
    {
        if (string.IsNullOrEmpty(guid))
            guid = Guid.NewGuid().ToString();

        sceneName = gameObject.scene.name; // 👈 Ghi scene hiện tại
    }

    private void Awake()
    {
        // đảm bảo luôn đúng scene
        sceneName = gameObject.scene.name;
    }
}
