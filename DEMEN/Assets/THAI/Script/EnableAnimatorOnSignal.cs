using UnityEngine;

public class EnableAnimatorOnSignal : MonoBehaviour
{
    public Animator animator;

    public void EnableAnimator()
    {
        animator.enabled = true;
    }
}
