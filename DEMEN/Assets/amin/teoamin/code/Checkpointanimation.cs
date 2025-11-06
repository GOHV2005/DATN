// CheckpointVisual.cs
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
public class CheckpointVisual : MonoBehaviour
{
    [Header("References")]
    public Transform rotatingObject;
    public float rotationSpeed = 30f;

    [Header("Animation")]
    public string normalAnimName = "checkpoint(nomal)";
    public string activeAnimName = "checkpoint(active)";

    private Animator animator;
    private bool playerInRange = false;

    // Danh sách tất cả checkpoint (tự quản lý)
    private static List<CheckpointVisual> allCheckpoints = new List<CheckpointVisual>();
    private static CheckpointVisual activeCheckpoint = null;

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator not found on Checkpoint!");
        }

        // Thêm vào danh sách toàn cục
        allCheckpoints.Add(this);

        PlayNormalAnimation();
    }

    void OnDestroy()
    {
        // Xoá khỏi danh sách khi bị destroy
        allCheckpoints.Remove(this);
        if (activeCheckpoint == this)
            activeCheckpoint = null;
    }

    void Update()
    {
        if (rotatingObject != null)
        {
            rotatingObject.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }

        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ActivateThisCheckpoint();
        }
    }

    public void SetPlayerInRange(bool inRange)
    {
        playerInRange = inRange;
    }

    void ActivateThisCheckpoint()
    {
        // Nếu đã là checkpoint active → không làm gì
        if (activeCheckpoint == this) return;

        // Tắt checkpoint cũ (nếu có)
        if (activeCheckpoint != null)
        {
            activeCheckpoint.Deactivate();
        }

        // Kích hoạt checkpoint này
        activeCheckpoint = this;
        PlayActiveAnimation();
        Debug.Log($"Checkpoint activated at {transform.position}");

        // Gọi sự kiện toàn cục (nếu cần tích hợp với SaveSystem)
        OnCheckpointActivated?.Invoke(this);
    }

    void Deactivate()
    {
        PlayNormalAnimation();
    }

    void PlayNormalAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(normalAnimName))
        {
            animator.Play(normalAnimName);
        }
    }

    void PlayActiveAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(activeAnimName))
        {
            animator.Play(activeAnimName);
        }
    }

    // Sự kiện để các hệ thống khác lắng nghe (tuỳ chọn)
    public static System.Action<CheckpointVisual> OnCheckpointActivated;
}