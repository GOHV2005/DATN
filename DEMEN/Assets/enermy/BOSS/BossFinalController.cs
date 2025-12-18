using System.Collections;
using UnityEngine;

public class BossFinalController : MonoBehaviour
{
    public Collider2D talkTrigger;
    public Dialogue introDialogue;
    public Dialogue outroDialogue;

    public BossSpawner bossSpawner;
    public MonoBehaviour bossAI;
    public GameObject doorBlockerPrefab;
    public Transform doorSpawnPoint;
    public GameObject PanelDead;
    public CanvasGroup panelDeadCanvas;
    public float fadeDuration = 1.5f;



    private bool hasStarted = false;

    void Awake()
    {
        if (bossAI != null)
            bossAI.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasStarted) return;
        if (!other.CompareTag("Player")) return;

        hasStarted = true;
        StartCoroutine(IntroFlow());
    }

    IEnumerator IntroFlow()
    {
        Debug.Log("🗣️ [INTRO] Start");

        UIManager.IsTalkingToNPC = true;
        Debug.Log("🗣️ IsTalkingToNPC = TRUE");

        DialogueSystem.Instance.StartDialogue(introDialogue);
        Debug.Log("🗣️ Dialogue started");

        yield return new WaitUntil(() => !UIManager.IsTalkingToNPC);

        Debug.Log("🗣️ Dialogue REALLY finished");

        // 🚪 Door
        if (doorBlockerPrefab && doorSpawnPoint)
        {
            Instantiate(doorBlockerPrefab, doorSpawnPoint.position, Quaternion.identity);
            Debug.Log("🚪 Door blocked");
        }

        // ⚔️ Boss AI
        if (bossAI != null)
        {
            bossAI.enabled = true;
            Debug.Log("🤖 Boss AI enabled");
        }

        // ⚔️ Combat
        if (bossSpawner != null)
        {
            Debug.Log("🔥 Calling StartCombat()");
            bossSpawner.StartCombat();
        }
        else
        {
            Debug.LogError("❌ bossSpawner NULL");
        }
    }



    bool DialogueEnded()
    {
        // dựa trên cờ bạn đã có
        return !UIManager.IsTalkingToNPC;
    }

    public void OnBossDefeated()
    {
        
        StartCoroutine(OutroFlow());
    }

    IEnumerator OutroFlow()
    {
        if (bossAI != null)
            bossAI.enabled = false;

        UIManager.IsTalkingToNPC = true;
        DialogueSystem.Instance.StartDialogue(outroDialogue);

        yield return new WaitUntil(() => DialogueEnded());

        UIManager.IsTalkingToNPC = false;

        // 🌑 BẬT PANEL TRƯỚC
        PanelDead.SetActive(true);

        // reset alpha đề phòng chạy lại
        panelDeadCanvas.alpha = 0f;

        // 🌑 Fade to black
        yield return StartCoroutine(FadePanel(panelDeadCanvas, 1f));

        // 🎬 Load scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("CutEnd");
    }


    IEnumerator FadePanel(CanvasGroup canvas, float targetAlpha)
    {
        float startAlpha = canvas.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        canvas.alpha = targetAlpha;
    }

}
