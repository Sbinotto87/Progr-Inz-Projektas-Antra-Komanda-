using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public float weight;
    public Sprite icon;
    public bool isStackable;
}
