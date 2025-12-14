using UnityEngine;

public class MinigameGateController : MonoBehaviour
{
    [Header("References")]
    public GameObject gateObject;       // Cổng minigame
    public GameObject switchObject;     // Công tắc
    public Transform logObject;         // Khúc cây chắn đường
    public Vector3 logMoveTarget;       // Vị trí cây sẽ chạy đến
    public float logMoveSpeed = 2f;     // Tốc độ cây chạy

    [Header("Switch Animation")]
    public float switchMoveDistance = 0.2f; // Khoảng cách gạt xuống
    public float switchMoveSpeed = 2f;      // Tốc độ gạt

    private bool minigameWon = false;       // Không static nữa
    private bool switchActivated = false;
    private bool switchAnimating = false;
    private Vector3 switchInitialPos;
    private Vector3 switchTargetPos;

    private void Start()
    {
        if (switchObject != null)
        {
            switchInitialPos = switchObject.transform.position;
            switchTargetPos = switchInitialPos + Vector3.down * switchMoveDistance;
        }

        // Reset trạng thái dựa trên minigameWon
        if (minigameWon)
        {
            if (gateObject != null) gateObject.SetActive(false);
            if (switchObject != null) switchObject.SetActive(true);
            if (logObject != null) logObject.position = logMoveTarget;
        }
        else
        {
            if (gateObject != null) gateObject.SetActive(true);
            if (switchObject != null) switchObject.SetActive(false);
            if (logObject != null) logObject.position = logObject.position; // giữ vị trí ban đầu
        }
    }

    private void Update()
    {
        // Animation công tắc
        if (switchAnimating && switchObject != null)
        {
            switchObject.transform.position = Vector3.MoveTowards(
                switchObject.transform.position, switchTargetPos, switchMoveSpeed * Time.deltaTime);

            if (Vector3.Distance(switchObject.transform.position, switchTargetPos) < 0.01f)
            {
                switchAnimating = false;
                switchActivated = true;
                if (switchObject != null) switchObject.SetActive(false);
            }
        }

        // Di chuyển khúc cây
        if (switchActivated && logObject != null)
        {
            logObject.position = Vector3.MoveTowards(
                logObject.position, logMoveTarget, logMoveSpeed * Time.deltaTime);
        }
    }

    // Gọi khi thắng minigame
    public void OnMinigameWon()
    {
        minigameWon = true;

        if (gateObject != null) gateObject.SetActive(false);
        if (switchObject != null) switchObject.SetActive(true);
    }

    // Nhấn công tắc
    public void ActivateSwitch()
    {
        if (switchActivated || switchAnimating) return;
        switchAnimating = true;
    }
}
