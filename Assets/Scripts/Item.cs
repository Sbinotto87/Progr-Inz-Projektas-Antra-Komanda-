using UnityEngine;

public enum ItemCategory
{
    Tool,
    Weapon,
    Food,
    Drink,
    Block,
    Misc 
}

[CreateAssetMenu(fileName = "Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    [Header("Categorization")]
    public ItemCategory category;

    public string itemName;
    public float weight;
    public Sprite icon;
    public bool isStackable;
    public int blockIndex;

    [Header("Consumable Stats")]
    public float hungerRestoreValue;
}
