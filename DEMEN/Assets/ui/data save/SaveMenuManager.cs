using UnityEngine;
using UnityEngine.UI;

public class SaveMenuManager : MonoBehaviour
{
    public SaveSlotUI[] slots; // gán trong Inspector
    public Button deleteButton;

    private SaveSlotUI selectedSlot;

    void Start()
    {
        deleteButton.gameObject.SetActive(false);
        deleteButton.onClick.AddListener(OnDeleteButtonClick);
        RefreshAllSlots();
    }

    public void SelectSlot(SaveSlotUI slot)
    {
        selectedSlot = slot;
        deleteButton.gameObject.SetActive(true);
    }

    private void OnDeleteButtonClick()
    {
        if (selectedSlot != null)
        {
            int index = selectedSlot.GetSlotIndex();
            SaveSystem.DeleteSlot(index);

            if (PlayerPrefs.GetInt("CurrentSlot", -1) == index)
                PlayerPrefs.DeleteKey("CurrentSlot");

            selectedSlot.Deselect();
            selectedSlot = null;

            RefreshAllSlots();
            deleteButton.gameObject.SetActive(false);
        }
    }

    private void RefreshAllSlots()
    {
        foreach (var slot in slots)
            slot.LoadSlot();
    }
}
