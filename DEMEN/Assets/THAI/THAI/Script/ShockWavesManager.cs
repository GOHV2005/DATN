using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShockWavesManager : MonoBehaviour
{
    [Header("Shockwave Settings")]
    [SerializeField] private float shockwaveVisualDuration = 1f;   // fast visual
    [SerializeField] private float slowMotionDuration = 5f;        // world slowdown
    [SerializeField] private float cooldownTime = 10f;
    [SerializeField] private float slowMotionFactor = 0.2f;        // 0 = freeze, 1 = normal

    private SpriteRenderer _spriteRenderer;
    private MaterialPropertyBlock _propBlock;
    private static readonly int WavesDistanceFromCenter = Shader.PropertyToID("_WavesDistanceFromCenter");

    private bool _isOnCooldown = false;
    private float _lastShockwaveTime = -Mathf.Infinity;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _propBlock = new MaterialPropertyBlock();
    }

    private void Update()
    {
        //if (Keyboard.current.eKey.wasPressedThisFrame && !_isOnCooldown)
        //{
        //    CallShockWaves();
        //}
    }
    public void TriggerShockwaveFromBoss()
    {
        CallShockWaves();
    }

    public void CallShockWaves()
    {
        if (Time.time - _lastShockwaveTime < cooldownTime) return;

        _lastShockwaveTime = Time.time;
        StartCoroutine(ShockwaveRoutine(-0.1f, 1f));
    }

    private IEnumerator ShockwaveRoutine(float startPos, float endPos)
    {
        _isOnCooldown = true;

        // Phase 1: Shockwave visual (1 second)
        yield return StartCoroutine(ShockwaveVisual(startPos, endPos));

        // Phase 2: Slow down the world (5 seconds)
        //yield return StartCoroutine(SlowMotionWorld());

        _isOnCooldown = false;
    }

    private IEnumerator ShockwaveVisual(float startPos, float endPos)
    {
        float elapsed = 0f;

        while (elapsed < shockwaveVisualDuration)
        {
            elapsed += Time.unscaledDeltaTime; // unaffected by slowdown
            float lerped = Mathf.Lerp(startPos, endPos, elapsed / shockwaveVisualDuration);

            // Apply property via MaterialPropertyBlock
            _spriteRenderer.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat(WavesDistanceFromCenter, lerped);
            _spriteRenderer.SetPropertyBlock(_propBlock);

            yield return null;
        }
    }

    //private IEnumerator SlowMotionWorld()
    //{
    //    float originalScale = Time.timeScale;
    //    Time.timeScale = slowMotionFactor;

    //    yield return new WaitForSecondsRealtime(slowMotionDuration);

    //    Time.timeScale = originalScale;
    //}
}
