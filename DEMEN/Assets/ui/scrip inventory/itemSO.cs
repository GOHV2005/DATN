using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class itemSO : ScriptableObject
{
    public string itemName;
    public StatToChange statToChange;
    public int amountToChangeStat;

    public AttributeToChange attributeToChange;
    public int amountToChangeAttrinute;

    public bool UseItem()
    {
        if (statToChange == StatToChange.health)
        {
            if (PlayerController.Instance == null)
            {
                Debug.LogError("Player not found!");
                return false;
            }

            // Kiểm tra full health
            if (PlayerController.Instance.CurrentHealth >= PlayerController.Instance.maxHealth)
            {
                return false;
            }

            // Gọi hàm Heal (đã có sẵn trong PlayerController)
            PlayerController.Instance.Heal(amountToChangeStat);
            return true;
        }

        // Thêm mana sau nếu cần
        if (statToChange == StatToChange.mana)
        {
            if (PlayerController.Instance == null) return false;
            PlayerController.Instance.RestoreMana(amountToChangeStat);
            return true;
        }

        return false;
    }

    public enum StatToChange
    {
        none,
        health,
        mana,
        stamina
    }

    public enum AttributeToChange
    {
        none,
        strength,
        defense,
        intelligence,
        agility
    }
}