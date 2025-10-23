using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DragItem : MonoBehaviour
{
    public Image icon;
    public TMP_Text quantityText;

    public void Setup(Sprite sprite, int quantity)
    {
        icon.sprite = sprite;
        quantityText.text = quantity > 1 ? quantity.ToString() : "";
    }
}
