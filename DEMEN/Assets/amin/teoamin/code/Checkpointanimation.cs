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

    private static List<CheckpointVisual> allCheckpoints = new List<CheckpointVisual>();
    private static CheckpointVisual activeCheckpoint = null;

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator not found on Checkpoint!");
        }
        allCheckpoints.Add(this);
        PlayNormalAnimation();
    }

    void OnDestroy()
    {
        allCheckpoints.Remove(this);
        if (activeCheckpoint == this)
            activeCheckpoint = null;
    }

    void Update()
    {
        // Xử lý quay
        if (rotatingObject != null)
        {
            rotatingObject.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }

        // Xử lý nhấn E khi player đang trong vùng
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ActivateThisCheckpoint();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    public void SetPlayerInRange(bool inRange)
    {
        playerInRange = inRange;
    }

    void ActivateThisCheckpoint()
    {
        if (activeCheckpoint == this) return;

        if (activeCheckpoint != null)
        {
            activeCheckpoint.Deactivate();
        }

        activeCheckpoint = this;
        PlayActiveAnimation();
        Debug.Log($"Checkpoint activated at {transform.position}");
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

    public static System.Action<CheckpointVisual> OnCheckpointActivated;
}