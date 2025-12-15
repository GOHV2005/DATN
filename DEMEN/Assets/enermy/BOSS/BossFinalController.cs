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

        UnityEngine.SceneManagement.SceneManager.LoadScene("END");
    }
}
