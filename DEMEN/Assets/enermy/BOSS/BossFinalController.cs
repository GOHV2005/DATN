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
        if (!other.CompareTag("Player")) return;
        if (hasStarted) return;

        hasStarted = true;

        // 🔁 ĐÃ TỪNG ĐÁNH → BỎ QUA HỘI THOẠI
        if (BossFightState.introFinished)
        {
            Debug.Log("⚔️ Boss retry → vào combat ngay");

            if (doorBlockerPrefab && doorSpawnPoint)
                Instantiate(doorBlockerPrefab, doorSpawnPoint.position, Quaternion.identity);

            if (bossAI != null)
                bossAI.enabled = true;

            if (bossSpawner != null)
            {
                bossSpawner.allowCombat = true;
                bossSpawner.StartCombat();
            }

            return;
        }

        // 🆕 CHƠI MỚI → CÓ HỘI THOẠI
        StartCoroutine(IntroFlow());
    }

    // Trong BossFinalController.cs

    IEnumerator IntroFlow()
    {
        UIManager.IsTalkingToNPC = true;
        DialogueSystem.Instance.StartDialogue(introDialogue);
        yield return new WaitUntil(() => !UIManager.IsTalkingToNPC);

        BossFightState.introFinished = true;

        // 💾 Lưu vào save
        int slot = PlayerPrefs.GetInt("CurrentSlot", -1);
        if (slot != -1)
        {
            SaveData data = SaveSystem.LoadGame(slot);
            if (data != null)
            {
                data.bossIntroFinished = true;
                SaveSystem.SaveGame(slot, data);
            }
        }

        // 🔒 ĐÓNG CỬA
        if (doorBlockerPrefab && doorSpawnPoint)
            Instantiate(doorBlockerPrefab, doorSpawnPoint.position, Quaternion.identity);

        // 👇 MỚI: HỖ TRỢ BOSS CÓ INTRO
        if (bossAI != null)
        {
            bossAI.enabled = true;

            // Nếu boss có phương thức StartIntroSequence → gọi nó
            if (bossAI is BossMantisAI mantis)
            {
                mantis.StartIntroSequence();
            }
            else if (bossAI is BossBeetleAI beetle)
            {
                beetle.StartIntroSequence();
            }
            // Nếu không → giả sử nó đã sẵn sàng combat
        }

        // 🔓 MỞ COMBAT
        if (bossSpawner != null)
        {
            bossSpawner.allowCombat = true;
            bossSpawner.StartCombat();
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
