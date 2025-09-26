using UnityEngine;
using UnityEngine.UI;

public class SaveMenuManager : MonoBehaviour
{
    public SaveSlotUI[] slots; // gán 3 slots trong Inspector
    public Button deleteButton; // gán nút Delete chung trong Inspector

    private SaveSlotUI selectedSlot;

    void Start()
    {
        deleteButton.gameObject.SetActive(false);
        deleteButton.onClick.AddListener(OnDeleteButtonClick);
    }

    public void SelectSlot(SaveSlotUI slot)
    {
        // bỏ chọn tất cả slot trước đó
        foreach (var s in slots)
        {
            s.Deselect();
        }

        // chọn slot mới
        selectedSlot = slot;
        deleteButton.gameObject.SetActive(true);
    }

    private void OnDeleteButtonClick()
    {
        if (selectedSlot != null)
        {
            int index = selectedSlot.GetSlotIndex();
            SaveSystem.DeleteSave(index);
            selectedSlot.Deselect();
            selectedSlot.LoadSlot();

            deleteButton.gameObject.SetActive(false);
        }
    }
}
