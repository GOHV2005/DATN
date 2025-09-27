using UnityEngine;
using UnityEngine.UI;

public class SaveMenuManager : MonoBehaviour
{
    public SaveSlotUI[] slots; // gán 4 slots trong Inspector
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

        // Nếu slot có dữ liệu mới cho xóa
        deleteButton.gameObject.SetActive(selectedSlot.HasData());
    }

    public SaveSlotUI GetSelectedSlot()
    {
        return selectedSlot;
    }

    private void OnDeleteButtonClick()
    {
        if (selectedSlot != null && selectedSlot.HasData())
        {
            int index = selectedSlot.GetSlotIndex();
            SaveSystem.DeleteSlot(index);

            // Nếu đang xóa slot hiện tại → reset PlayerPrefs
            if (PlayerPrefs.GetInt("CurrentSlot", -1) == index)
                PlayerPrefs.DeleteKey("CurrentSlot");

            selectedSlot = null;
            deleteButton.gameObject.SetActive(false);
            RefreshAllSlots();
        }
    }

    private void RefreshAllSlots()
    {
        foreach (var slot in slots)
            slot.LoadSlot();
    }
}
