// DialogueManager.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("Data")]
    public Dialogue dialogue;
    private int currentNodeIndex;

    [Header("UI")]
    public GameObject panel;
    public TextMeshProUGUI npcText;
    public Button[] choiceButtons;

    [Header("Minigame")]
    public BellShuffleGameManager bellShuffle; // Assign in Inspector

    void Start()
    {
        if (bellShuffle != null)
        {
            bellShuffle.onMinigameComplete.AddListener(OnMinigameComplete);
            bellShuffle.onConversationSignal.AddListener(OnMinigameSignal);
        }
    }

    void Awake()
    {
        if (panel != null) panel.SetActive(false);
        if (bellShuffle != null) bellShuffle.gameObject.SetActive(false);
    }

    public void StartDialogue(Dialogue newDialogue, int startIndex = 0)
    {
        dialogue = newDialogue;
        currentNodeIndex = startIndex;
        if (panel != null) panel.SetActive(true);
        ShowNode(currentNodeIndex);
    }

    public void EndDialogue()
    {
        // Hide dialogue UI
        if (panel != null) panel.SetActive(false);
        //dialogue = null;

        // Show and start minigame
        if (bellShuffle != null)
        {
            bellShuffle.gameObject.SetActive(true);
            bellShuffle.StartMinigame();
        }
    }



    void ShowNode(int index)
    {
        if (dialogue == null || dialogue.nodes == null || index < 0 || index >= dialogue.nodes.Length)
        {
            EndDialogue();
            return;
        }

        currentNodeIndex = index;
        var node = dialogue.nodes[index];
        npcText.text = node.npcLine;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (node.playerChoices != null && i < node.playerChoices.Length)
            {
                var btn = choiceButtons[i];
                btn.gameObject.SetActive(true);

                var label = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null) label.text = node.playerChoices[i].choiceText;

                int nextIndex = node.playerChoices[i].nextNodeIndex;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => {
                    if (nextIndex < 0) EndDialogue();
                    else ShowNode(nextIndex);
                });
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnMinigameComplete(bool win)
    {
        if (bellShuffle != null) bellShuffle.gameObject.SetActive(false);

        // Pick the correct dialogue node based on outcome
        int winNode = 4;  // replace with your actual "Win" node index
        int loseNode = 5; // replace with your actual "Lose" node index
        int nextIndex = win ? winNode : loseNode;

        // Resume dialogue so NPC speaks
        if (dialogue != null)
        {
            panel.SetActive(true);
            ShowNode(nextIndex);
        }
        else
        {
            panel.SetActive(false);
        }
    }



    private void OnMinigameSignal(string key)
    {
        if (key == "MinigameWin")
        {
            // reward player, trigger quest, etc.
        }
        else if (key == "MinigameLose")
        {
            // consequences
        }
    }
}
