using System;
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
public class Item : ScriptableObject, IEquatable<Item>
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
    public float thirstRestoreValue;
    
    public bool Equals(Item other) => itemName.Equals(other.itemName);
}

public class CreatedItems : MonoBehaviour
{
    public Item[] items;

    public void Awake()
    {
        items = new Item[12];
        
        Item stone = ScriptableObject.CreateInstance<Item>();
        stone.category = ItemCategory.Block;
        stone.itemName = "Stone block";
        stone.weight = 10;
        stone.blockIndex = 0;
        stone.isStackable = true;
        items[0] = stone;
        
        Item grassBlock = ScriptableObject.CreateInstance<Item>();
        grassBlock.category = ItemCategory.Block;
        grassBlock.itemName = "Grass block";
        grassBlock.weight = 10;
        grassBlock.blockIndex = 1;
        grassBlock.isStackable = true;
        items[1] = grassBlock;
        
        Item wood = ScriptableObject.CreateInstance<Item>();
        wood.category = ItemCategory.Block;
        wood.itemName = "Wood block";
        wood.weight = 10;
        wood.blockIndex = 2;
        wood.isStackable = true;
        items[2] = wood;
        
        Item leaf = ScriptableObject.CreateInstance<Item>();
        leaf.category = ItemCategory.Block;
        leaf.itemName = "Leaf block";
        leaf.weight = 10;
        leaf.blockIndex = 3;
        leaf.isStackable = true;
        items[3] = leaf;
        
        Item grass = ScriptableObject.CreateInstance<Item>();
        grass.category = ItemCategory.Block;
        grass.itemName = "Grass";
        grass.weight = 10;
        grass.blockIndex = 4;
        grass.isStackable = true;
        items[4] = grass;
        
        Item IronPickaxe = ScriptableObject.CreateInstance<Item>();
        IronPickaxe.category = ItemCategory.Tool;
        IronPickaxe.itemName = "Iron pickaxe";
        IronPickaxe.weight = 10;
        IronPickaxe.icon = Resources.Load("Iron_pickaxe", typeof(Sprite)) as Sprite;
        items[5] = IronPickaxe;
        
        Item sword = ScriptableObject.CreateInstance<Item>();
        sword.category = ItemCategory.Weapon;
        sword.itemName = "Sword";
        sword.weight = 10;
        sword.icon = Resources.Load("diamond_sword", typeof(Sprite)) as Sprite;
        items[6] = sword;
        
        Item stonePickaxe = ScriptableObject.CreateInstance<Item>();
        stonePickaxe.category = ItemCategory.Tool;
        stonePickaxe.itemName = "Stone pickaxe";
        stonePickaxe.weight = 10;
        stonePickaxe.icon = Resources.Load("Iron_pickaxe", typeof(Sprite)) as Sprite;
        items[7] = stonePickaxe;
        
        Item IronOre = ScriptableObject.CreateInstance<Item>();
        IronOre.category = ItemCategory.Block;
        IronOre.itemName = "Iron Ore";
        IronOre.weight = 2;
        IronOre.isStackable = true;
        IronOre.blockIndex = 9;
        items[8] = IronOre;
                
        Item chestblock = ScriptableObject.CreateInstance<Item>();
        chestblock.category = ItemCategory.Block;
        chestblock.itemName = "Chest block";
        chestblock.weight = 5;
        chestblock.isStackable = true;
        chestblock.blockIndex = 12;
        items[9] = chestblock;
        
        Item beer = ScriptableObject.CreateInstance<Item>();
        beer.category = ItemCategory.Drink;
        beer.itemName = "Beer";
        beer.weight = 5;
        beer.isStackable = true;
        beer.hungerRestoreValue = 5;
        beer.thirstRestoreValue = 40;
        items[10] = beer;
        
        Item bread = ScriptableObject.CreateInstance<Item>();
        bread.category = ItemCategory.Food;
        bread.itemName = "Bread";
        bread.weight = 5;
        bread.isStackable = true;
        bread.hungerRestoreValue = 40;
        items[11] = bread;
    }
}