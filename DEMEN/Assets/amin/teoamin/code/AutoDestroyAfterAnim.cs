// AutoDestroyAfterAnim.cs
using UnityEngine;

public class AutoDestroyAfterAnim : MonoBehaviour
{
    public void Init(Vector3 position, bool isFacingRight)
    {
        // Đặt vị trí
        transform.position = position;

        // Xoay theo hướng player
        Vector3 scale = transform.localScale;
        scale.x = isFacingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;

        // Tự hủy sau animation
        if (TryGetComponent<Animator>(out Animator animator))
        {
            if (animator.runtimeAnimatorController != null &&
                animator.runtimeAnimatorController.animationClips.Length > 0)
            {
                AnimationClip clip = animator.runtimeAnimatorController.animationClips[0];
                Destroy(gameObject, clip.length);
            }
            else
            {
                Destroy(gameObject, 1f);
            }
        }
        else
        {
            Destroy(gameObject, 1f);
        }
    }
}