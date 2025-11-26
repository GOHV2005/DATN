using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class NPCController : MonoBehaviour
{
    public TextMeshProUGUI npcDialogueText;
    public Transform positionA;
    public GameObject npcObject;

    public void DecideResult(bool isWin)
    {
        if (isWin)
        {
            npcDialogueText.text = "Hay lắm!";
        }
        else
        {
            npcDialogueText.text = "Chúc may mắn lần sau!";
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                player.transform.position = positionA.position;
            }
        }

        // NPC biến mất sau 2 giây
        Invoke("Disappear", 2f);
    }

    void Disappear()
    {
        npcObject.SetActive(false); // hoặc Destroy(npcObject);
    }
}
