using UnityEngine;


[CreateAssetMenu]
public class itemSO : ScriptableObject
{
    public string itemName;
    public StatToChange statToChange = new  StatToChange();
    public int amountToChangeStat;

    public AttributeToChange attributeToChange = new AttributeToChange();
    public int amountToChangeAttrinute;


    public bool UseItem()
    {
        if(statToChange == StatToChange.health)
        {
            PlayerHealth playerHealth = GameObject.Find("HealthManager").GetComponent<PlayerHealth>();
            if(playerHealth.currentHealth == playerHealth.maxHealth)
            {
                return false;
            }
            else
            {
                playerHealth.RestoreHealth(amountToChangeStat);
                return true;
            }
                
        }/*
        if (statToChange == StatToChange.mana)
        {
            GameObject.Find("ManaManager").GetComponent<PlayerMana>().ChangeMana(amountToChangeStat);
        }*/
        return false;
    }

    public enum StatToChange
    {
        none,
        health,
        mana,
        stamina
    };

    public enum AttributeToChange
    {
        none,
        strength,
        defense,
        intelligence,
        agility
    };
}
