// SceneGateTrigger.cs
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneGateTrigger : MonoBehaviour
{
    [Header("=== Scene Settings ===")]
    public bool enableTransition = true;
    public string targetSceneName = "Level_2";
    public string requiredItemName = "Key";
    public bool consumeItemOnUse = true;

    [Header("=== UI Feedback ===")]
    public string denyMessage = "You need a key to open this door!";
    public float messageDuration = 2f;
    public float fadeDuration = 0.5f;
    public Vector3 messageOffset = new Vector3(0, 1.5f, 0);

    [Header("=== Font & Rendering ===")]
    public TMP_FontAsset customFont; // Kéo Assets/font/newfontnumber.asset vào đây

    [Header("=== Sorting (Để không bị che) ===")]
    public string sortingLayerName = "UI";       // 👈 Sorting Layer
    public int sortingOrder = 1000;              // 👈 Order in Layer
    [Header("=== Text Wrapping ===")]
    public float textWidth = 200f; // Kéo thanh trượt trong Inspector
    private TextMeshPro floatingText;

    void Start()
    {
        CreateFloatingText();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TryTransition();
        }
    }

    void TryTransition()
    {
        if (HasRequiredItem())
        {
            if (enableTransition) // 👈 CHỈ CHUYỂN SCENE NẾU BẬT
            {
                if (consumeItemOnUse)
                {
                    InventoryManager.Instance?.RemoveItem(requiredItemName, 1);
                }
                SaveCheckpoint();
                AutoSaveRAM.Instance?.Capture();
                SceneLoader.LoadScene(targetSceneName);
            }
            // Nếu enableTransition = false → KHÔNG LÀM GÌ CẢ (không hiện text)
        }
        else
        {
            // 👇 LUÔN HIỆN TEXT NẾU KHÔNG CÓ ITEM — DÙ enableTransition = true/false
            ShowMessage(denyMessage);
        }
    }

    bool HasRequiredItem()
    {
        if (InventoryManager.Instance == null || string.IsNullOrEmpty(requiredItemName))
            return false;

        foreach (var slot in InventoryManager.Instance.itemSlots)
        {
            if (slot.itemName == requiredItemName && slot.quantity > 0)
                return true;
        }
        return false;
    }

    void CreateFloatingText()
    {
        GameObject textObj = new GameObject("FloatingMessage");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = messageOffset;

        // 👇 THÊM RectTransform
        var rectTransform = textObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(15f, 5f); // 👈 ĐẶT KÍCH THƯỚC KHUNG

        floatingText = textObj.AddComponent<TextMeshPro>();
        floatingText.text = "";
        floatingText.fontSize = 8;
        floatingText.alignment = TextAlignmentOptions.Center;
        floatingText.color = Color.white;

        // 👇 GÁN FONT
        floatingText.font = customFont != null ? customFont : TMP_Settings.defaultFontAsset;

        // 👇 AUTO SIZE + WORD WRAP
        floatingText.enableAutoSizing = false;
        floatingText.fontSizeMin = 12;
        floatingText.fontSizeMax = 48;
        floatingText.enableWordWrapping = true; // ✅ BẬT WORD WRAP
        floatingText.overflowMode = TextOverflowModes.Overflow;

        // 👇 SẮP XẾP LỚP
        var textRenderer = floatingText.GetComponent<Renderer>();
        if (textRenderer != null)
        {
            textRenderer.sortingLayerName = sortingLayerName;
            textRenderer.sortingOrder = sortingOrder;
        }

        floatingText.gameObject.SetActive(false);
    }

    void ShowMessage(string msg)
    {
        if (floatingText == null) return;
        floatingText.text = msg;
        floatingText.gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(FadeInOut());
    }

    System.Collections.IEnumerator FadeInOut()
    {
        float total = messageDuration;
        float fade = fadeDuration;

        // Fade IN
        float t = 0;
        while (t < fade)
        {
            float alpha = Mathf.Lerp(0, 1, t / fade);
            floatingText.color = new Color(1, 1, 1, alpha);
            t += Time.deltaTime;
            yield return null;
        }

        // Giữ rõ
        yield return new WaitForSeconds(total - fade * 2);

        // Fade OUT
        t = 0;
        while (t < fade)
        {
            float alpha = Mathf.Lerp(1, 0, t / fade);
            floatingText.color = new Color(1, 1, 1, alpha);
            t += Time.deltaTime;
            yield return null;
        }

        floatingText.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (floatingText != null)
            Destroy(floatingText.gameObject);
    }
    private void SaveCheckpoint()
    {
        int slotIndex = PlayerPrefs.GetInt("CurrentSlot", 0);
        SaveData data = SaveSystem.LoadGame(slotIndex) ?? new SaveData();

        string currentScene = SceneManager.GetActiveScene().name;

        SceneSaveData checkpoint = new SceneSaveData
        {
            sceneName = currentScene,
            position = transform.position,
            playTime = PlayTimeTracker.Instance != null ? PlayTimeTracker.Instance.GetPlayTime() : 0f
        };

        // Lưu inventory
        InventoryManager invMgr = InventoryManager.Instance;
        if (invMgr != null)
        {
            data.inventory = invMgr.GetInventoryData();
            var player = PlayerController.Instance;

            if (player != null)
            {


                // 🔒 LƯU MÁU FULL VÀO SAVE
                data.playerHealth = player.CurrentHealth;

                data.inventory.isHoldingLongden = player.IsHoldingLongden;
                data.inventory.isHoldingCuocChim = player.IsHoldingCuocChim;
                data.inventory.isHoldingKiem = player.IsHoldingKiem;
            }
        }


        // ✅ Lưu SaveableObject chỉ của scene hiện tại
        data.saveableObjects.RemoveAll(o => o.sceneName == currentScene); // xóa bản lưu cũ của scene này
        foreach (var obj in FindObjectsOfType<SaveableObject>())
        {
            if (obj.sceneName != currentScene) continue;

            data.saveableObjects.Add(new SaveData.SaveableObjectRef(
                obj.guid,
                obj.sceneName,
                obj.GetComponent<SaveData.ISaveable>()?.CaptureState() // nếu có state
            ));
        }
        SaveSystem.SaveGame(slotIndex, data);
        PlayerPrefs.SetInt("CurrentSlot", slotIndex);

        Debug.Log($"[CHECKPOINT] Saved scene '{currentScene}' with {data.saveableObjects.Count} saveable objects.");
    }
}