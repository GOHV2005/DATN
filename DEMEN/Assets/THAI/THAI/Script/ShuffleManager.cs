using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

public class BellShuffleGameManager : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent<bool> onMinigameComplete;      // true=win, false=lose
    public UnityEvent<string> onConversationSignal;  // "MinigameWin" or "MinigameLose"

    [Header("Scene refs")]
    public List<BellController> bells;   // assign 3 bell objects
    public Transform[] positions;        // assign 3 fixed slot transforms
    public Transform ball;

    [Header("Timing")]
    public float revealDelay = 2f;
    public float postRevealPause = 1f;
    public float shuffleTotalTime = 5f;
    public float moveDuration = 0.35f;
    public float minStepDelay = 0.35f;
    public float maxStepDelay = 0.9f;

    public int ballIndex = 1;
    private bool _isShuffling;
    private bool _isResolving;
    private Coroutine shuffleRoutine;
    private Coroutine flowRoutine;

    private void Awake()
    {
        foreach (var bell in bells)
        {
            bell.onBellClicked += OnBellClicked;
            bell.SetInteractable(false);
        }
    }

    private void Start()
    {
        PrepareBall();
        StartMinigame();
    }

    public void StartMinigame()
    {
        if (flowRoutine != null) StopCoroutine(flowRoutine);
        flowRoutine = StartCoroutine(GameFlow());
    }

    private void PrepareBall()
    {
        var anchor = bells[ballIndex].GetBallAnchor();
        ball.position = anchor.position;
    }

    private IEnumerator GameFlow()
    {
        // reveal ball
        bells[ballIndex].LiftAndReveal();
        yield return new WaitForSeconds(revealDelay);

        // short pause
        yield return new WaitForSeconds(postRevealPause);

        // shuffle
        shuffleRoutine = StartCoroutine(ShuffleRoutine());
        yield return shuffleRoutine;

        // enable choice
        EnablePlayerChoice(true);
    }

    private IEnumerator ShuffleRoutine()
    {
        _isShuffling = true;
        float elapsedTime = 0f;

        while (elapsedTime < shuffleTotalTime)
        {
            float t = elapsedTime / shuffleTotalTime;
            float curve = 4f * t * (1f - t); // parabola
            float stepDelay = Mathf.Lerp(minStepDelay, maxStepDelay, 1f - curve);

            // random permutation of slot positions
            var shuffledPositions = positions.OrderBy(x => Random.value).ToArray();

            // move all bells in parallel
            yield return MoveAllBellsParallel(shuffledPositions, moveDuration);

            yield return new WaitForSeconds(stepDelay);
            elapsedTime += stepDelay;
        }

        _isShuffling = false;
    }

    private IEnumerator MoveAllBellsParallel(Transform[] targetPositions, float duration)
    {
        float elapsed = 0f;
        Vector3[] startPositions = new Vector3[bells.Count];

        for (int i = 0; i < bells.Count; i++)
            startPositions[i] = bells[i].transform.position;

        while (elapsed < duration)
        {
            float alpha = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            for (int i = 0; i < bells.Count; i++)
            {
                bells[i].transform.position = Vector3.Lerp(startPositions[i], targetPositions[i].position, alpha);
                bells[i].transform.rotation = Quaternion.identity;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < bells.Count; i++)
        {
            bells[i].transform.position = targetPositions[i].position;
            bells[i].transform.rotation = Quaternion.identity;
        }
    }

    private void EnablePlayerChoice(bool enable)
    {
        foreach (var bell in bells) bell.SetInteractable(enable);
    }

    private void OnBellClicked(BellController clicked)
    {
        if (_isShuffling || _isResolving) return;

        _isResolving = true;
        EnablePlayerChoice(false);

        int chosenIndex = bells.IndexOf(clicked);
        bool win = chosenIndex == ballIndex;

        clicked.LiftAndReveal(() => ResolveOutcome(win));
    }

    private void ResolveOutcome(bool win)
    {
        onMinigameComplete?.Invoke(win);
        onConversationSignal?.Invoke(win ? "MinigameWin" : "MinigameLose");

        if (!win)
        {
            bells[ballIndex].LiftAndReveal();
        }

        StartCoroutine(EndSequence());
    }

    private IEnumerator EndSequence()
    {
        yield return new WaitForSeconds(0.8f);
        _isResolving = false;
    }
}

