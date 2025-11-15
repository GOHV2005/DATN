using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterEffect : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    public float delay = 0.05f;
    private string fullText;

    void Start()
    {
        fullText = textMesh.text;
        textMesh.text = "";
        StartCoroutine(ShowText());
    }

    IEnumerator ShowText()
    {
        foreach (char c in fullText)
        {
            textMesh.text += c;
            yield return new WaitForSeconds(delay);
        }
    }
}
