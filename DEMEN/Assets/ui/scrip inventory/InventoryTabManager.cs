using UnityEngine;
using UnityEngine.UI;

public class InventoryTabManager : MonoBehaviour
{
    public GameObject mapPanel;
    public GameObject equipmentPanel;
    public GameObject inventoryPanel;

    public void ShowMap() { mapPanel.SetActive(true); equipmentPanel.SetActive(false); inventoryPanel.SetActive(false); }
    public void ShowEquipment() { mapPanel.SetActive(false); equipmentPanel.SetActive(true); inventoryPanel.SetActive(false); }
    public void ShowInventory() { mapPanel.SetActive(false); equipmentPanel.SetActive(false); inventoryPanel.SetActive(true); }
}
