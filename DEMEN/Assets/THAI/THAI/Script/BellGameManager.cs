using UnityEngine;
using System.Collections;
using System.Linq;
using TMPro;

public class BellGameManager : MonoBehaviour
{
    public NPCController nPCController; // if used elsewhere
    [Header("Game Setup")]
    public GameObject[] bells;              // assign 3 bell GameObjects
    public Transform[] positions;           // assign 3 fixed positions
    public AudioSource winMusic;
    public Transform positionA;             // teleport location
    public TextMeshProUGUI messageText;
    public GameObject prizeUI;
    public GameObject uiCanvas;

    [Header("Timing")]
    public float revealDelay = 2f;          // time before revealing real bell
    public float postRevealPause = 1f;      // time after reveal before shuffle
    public float shuffleTotalTime = 5f;     // total shuffle duration
    public float moveDuration = 0.35f;      // lerp duration per move
    public float minStepDelay = 0.35f;      // slow at start/end
    public float maxStepDelay = 0.9f;       // slow at extremes (inverted by curve)

    private int realBellIndex;
    private Coroutine shuffleRoutine;
    private Coroutine flowRoutine;

    void Start()
    {
        if (uiCanvas != null) uiCanvas.SetActive(false); // hide at start
    }

    public void ShowGame()
    {
        if (uiCanvas != null) uiCanvas.SetActive(true);
        if (flowRoutine != null) StopCoroutine(flowRoutine);
        flowRoutine = StartCoroutine(GameFlow());
    }

    public void HideGame()
    {
        if (uiCanvas != null) uiCanvas.SetActive(false);
        if (shuffleRoutine != null) StopCoroutine(shuffleRoutine);
        if (flowRoutine != null) StopCoroutine(flowRoutine);
    }

    IEnumerator GameFlow()
    {
        // pick real bell
        realBellIndex = Random.Range(0, bells.Length);
        messageText.text = "Chuẩn bị...";
        prizeUI.SetActive(false);

        // wait, then reveal
        yield return new WaitForSeconds(revealDelay);
        messageText.text = "Chuông thật là số " + (realBellIndex + 1);

        // short pause for player to see
        yield return new WaitForSeconds(postRevealPause);

        // start shuffle
        shuffleRoutine = StartCoroutine(ShuffleRoutine());

        // wait for shuffle to complete
        yield return shuffleRoutine;

        // prompt choice
        messageText.text = "Chọn chuông!";
    }

    IEnumerator ShuffleRoutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < shuffleTotalTime)
        {
            float t = elapsedTime / shuffleTotalTime;

            // curve: fast in middle, slow at start & end (parabola peak at 0.5)
            float curve = 4f * t * (1f - t);

            // inverse relationship: when curve high (fast), delay low
            float stepDelay = Mathf.Lerp(minStepDelay, maxStepDelay, 1f - curve);

            // compute target positions (random permutation)
            var shuffledPositions = positions.OrderBy(x => Random.value).ToArray();

            // move all bells in parallel, wait for them to finish
            yield return MoveAllBellsParallel(shuffledPositions, moveDuration);

            // delay between steps (separate from move time)
            yield return new WaitForSeconds(stepDelay);
            elapsedTime += stepDelay;
        }
    }

    IEnumerator MoveAllBellsParallel(Transform[] targetPositions, float duration)
    {
        float elapsed = 0f;
        Vector3[] startPositions = new Vector3[bells.Length];

        for (int i = 0; i < bells.Length; i++)
            startPositions[i] = bells[i].transform.position;

        while (elapsed < duration)
        {
            float alpha = elapsed / duration;
            for (int i = 0; i < bells.Length; i++)
            {
                bells[i].transform.position = Vector3.Lerp(startPositions[i], targetPositions[i].position, alpha);
                bells[i].transform.rotation = Quaternion.identity;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < bells.Length; i++)
        {
            bells[i].transform.position = targetPositions[i].position;
            bells[i].transform.rotation = Quaternion.identity;
        }
    }


    IEnumerator MoveBell(GameObject bell, Vector3 targetPos, float duration)
    {
        Transform tf = bell.transform;
        Vector3 startPos = tf.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float alpha = elapsed / duration;
            tf.position = Vector3.Lerp(startPos, targetPos, alpha);
            tf.rotation = Quaternion.identity; // keep upright
            elapsed += Time.deltaTime;
            yield return null;
        }

        tf.position = targetPos;
        tf.rotation = Quaternion.identity;
    }

    public void ChooseBell(int index)
    {
        if (shuffleRoutine != null) StopCoroutine(shuffleRoutine);

        bool isWin = (index == realBellIndex);

        if (isWin)
        {
            messageText.text = "✅ Chính xác!";
            prizeUI.SetActive(true);
            if (winMusic != null) winMusic.Play();
        }
        else
        {
            messageText.text = "❌ Đã sai!";
            prizeUI.SetActive(false);

            GameObject player = GameObject.FindWithTag("Player");
            if (player != null && positionA != null)
            {
                player.transform.position = positionA.position;
            }
        }

        // restart after short delay
        if (flowRoutine != null) StopCoroutine(flowRoutine);
        Invoke(nameof(RestartGame), 2f);
    }

    void RestartGame()
    {
        if (flowRoutine != null) StopCoroutine(flowRoutine);
        flowRoutine = StartCoroutine(GameFlow());
    }
}
