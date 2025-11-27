using UnityEngine;

public class DeleteLinkedObject : MonoBehaviour
{
    [Header("Object sẽ bị xóa cùng với object này")]
    public GameObject targetObject;

    void OnDestroy()
    {
        // Chỉ xóa nếu targetObject vẫn tồn tại (chưa bị xóa trước đó)
        if (targetObject != null)
        {
            Destroy(targetObject);
        }
    }
}