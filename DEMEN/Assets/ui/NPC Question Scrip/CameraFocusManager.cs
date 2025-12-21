using Unity.Cinemachine;
using UnityEngine;

public class CameraFocusManager : MonoBehaviour
{
    public static CameraFocusManager Instance;

    public CinemachineCamera vcam;
    public Transform playerTarget;

    private Transform originalFollow;
    private Transform originalLookAt;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        originalFollow = vcam.Follow;
        originalLookAt = vcam.LookAt;
    }

    public void FocusOn(Transform target)
    {
        if (target == null) return;

        vcam.Follow = target;
        vcam.LookAt = target;
    }

    public void ResetToPlayer()
    {
        vcam.Follow = playerTarget;
        vcam.LookAt = playerTarget;
    }
}
