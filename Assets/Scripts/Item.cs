using System;
using UnityEngine;

public enum ItemCategory
{
    Tool,
    Weapon,
    Food,
    Drink,
    Block,
    Misc,
    Ammo,
    Gun
}

[CreateAssetMenu(fileName = "Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject, IEquatable<Item>
{
    [Header("Categorization")]
    public ItemCategory category;
    public ToolCategory toolcategory;

    public string itemName;
    public float weight;
    public Sprite icon;
    public bool isStackable;
    public int blockIndex;
    public float toolEffectiveness;

    [Header("Consumable Stats")]
    public float hungerRestoreValue;
    public float thirstRestoreValue;
    public float healthRestoreValue;
    
    public bool Equals(Item other) => itemName.Equals(other.itemName);

    
}

public class CreatedItems : MonoBehaviour
{
    public Item[] items;

    public void Awake()
    {
        items = new Item[19];
        
        Item stone = ScriptableObject.CreateInstance<Item>();
        stone.category = ItemCategory.Block;
        stone.itemName = "Stone block";
        stone.weight = 5;
        stone.blockIndex = 0;
        stone.isStackable = true;
        stone.icon = Resources.Load("icons/stone_icon", typeof(Sprite)) as Sprite;
        items[0] = stone;
        
        Item grassBlock = ScriptableObject.CreateInstance<Item>();
        grassBlock.category = ItemCategory.Block;
        grassBlock.itemName = "Grass block";
        grassBlock.weight = 3;
        grassBlock.blockIndex = 1;
        grassBlock.isStackable = true;
        grassBlock.icon = Resources.Load("icons/grass_icon", typeof(Sprite)) as Sprite;
        items[1] = grassBlock;
        
        Item wood = ScriptableObject.CreateInstance<Item>();
        wood.category = ItemCategory.Block;
        wood.itemName = "Wood block";
        wood.weight = 1;
        wood.blockIndex = 2;
        wood.isStackable = true;
        wood.icon = Resources.Load("icons/wood_icon", typeof(Sprite)) as Sprite;
        items[2] = wood;
        
        Item leaf = ScriptableObject.CreateInstance<Item>();
        leaf.category = ItemCategory.Block;
        leaf.itemName = "Leaf block";
        leaf.weight = 1;
        leaf.blockIndex = 3;
        leaf.isStackable = true;
        leaf.icon = Resources.Load("icons/leaf_icon", typeof(Sprite)) as Sprite;
        items[3] = leaf;
        
        Item grass = ScriptableObject.CreateInstance<Item>();
        grass.category = ItemCategory.Block;
        grass.itemName = "Grass";
        grass.weight = 1;
        grass.blockIndex = 4;
        grass.isStackable = true;
        grass.icon = Resources.Load("icons/random_grass_icon", typeof(Sprite)) as Sprite;
        items[4] = grass;
        
        Item IronPickaxe = ScriptableObject.CreateInstance<Item>();
        IronPickaxe.category = ItemCategory.Tool;
        IronPickaxe.toolcategory = ToolCategory.Pickaxe;
        IronPickaxe.itemName = "Iron pickaxe";
        IronPickaxe.weight = 10;
        IronPickaxe.toolEffectiveness = 4.0f;
        IronPickaxe.icon = Resources.Load("iron_pickaxe", typeof(Sprite)) as Sprite;
        items[5] = IronPickaxe;
        
        Item sword = ScriptableObject.CreateInstance<Item>();
        sword.category = ItemCategory.Weapon;
        sword.itemName = "Sword";
        sword.weight = 10;
        sword.toolEffectiveness = 2.0f;
        sword.icon = Resources.Load("iron_sword", typeof(Sprite)) as Sprite;
        items[6] = sword;
        
        Item stonePickaxe = ScriptableObject.CreateInstance<Item>();
        stonePickaxe.category = ItemCategory.Tool;
        stonePickaxe.itemName = "Stone pickaxe";
        stonePickaxe.toolcategory = ToolCategory.Pickaxe;
        stonePickaxe.weight = 10;
        stonePickaxe.toolEffectiveness = 2.5f;
        stonePickaxe.icon = Resources.Load("stone_pickaxe", typeof(Sprite)) as Sprite;
        items[7] = stonePickaxe;
        
        Item IronOre = ScriptableObject.CreateInstance<Item>();
        IronOre.category = ItemCategory.Block;
        IronOre.itemName = "Iron Ore";
        IronOre.weight = 1;
        IronOre.isStackable = true;
        IronOre.blockIndex = 9;
        IronOre.icon = Resources.Load("icons/iron_icon", typeof(Sprite)) as Sprite;
        items[8] = IronOre;
                
        Item chestblock = ScriptableObject.CreateInstance<Item>();
        chestblock.category = ItemCategory.Block;
        chestblock.itemName = "Chest block";
        chestblock.weight = 5;
        chestblock.isStackable = true;
        chestblock.blockIndex = 12;
        chestblock.icon = Resources.Load("icons/chest_icon", typeof(Sprite)) as Sprite;
        items[9] = chestblock;
        
        Item beer = ScriptableObject.CreateInstance<Item>();
        beer.category = ItemCategory.Drink;
        beer.itemName = "Beer";
        beer.weight = 2;
        beer.isStackable = true;
        beer.hungerRestoreValue = 5;
        beer.thirstRestoreValue = 40;
        beer.healthRestoreValue = 50;
        beer.icon = Resources.Load("icons/beer_icon", typeof(Sprite)) as Sprite;
        items[10] = beer;
        
        Item bread = ScriptableObject.CreateInstance<Item>();
        bread.category = ItemCategory.Food;
        bread.itemName = "Bread";
        bread.weight = 2;
        bread.isStackable = true;
        bread.hungerRestoreValue = 40;
        bread.healthRestoreValue = 100;
        bread.icon = Resources.Load("icons/bread_icon", typeof(Sprite)) as Sprite;
        items[11] = bread;        
        
        Item Oil = ScriptableObject.CreateInstance<Item>();
        Oil.category = ItemCategory.Block;
        Oil.itemName = "Oil";
        Oil.weight = 2;
        Oil.isStackable = true;
        Oil.blockIndex = 11;
        Oil.icon = Resources.Load("icons/oil_icon", typeof(Sprite)) as Sprite;
        items[12] = Oil;
        
        Item stoneShovel = ScriptableObject.CreateInstance<Item>();
        stoneShovel.category = ItemCategory.Tool;
        stoneShovel.itemName = "Stone shovel";
        stoneShovel.toolcategory = ToolCategory.Shovel;
        stoneShovel.weight = 10;
        stoneShovel.toolEffectiveness = 2.5f;
        stoneShovel.icon = Resources.Load("stone_shovel", typeof(Sprite)) as Sprite;
        items[13] = stoneShovel;
        
        Item IronShovel = ScriptableObject.CreateInstance<Item>();
        IronShovel.category = ItemCategory.Tool;
        IronShovel.itemName = "Iron shovel";
        IronShovel.toolcategory = ToolCategory.Shovel;
        IronShovel.weight = 10;
        IronShovel.toolEffectiveness = 4f;
        IronShovel.icon = Resources.Load("iron_shovel", typeof(Sprite)) as Sprite;
        items[14] = IronShovel;
        
        Item stoneAxe = ScriptableObject.CreateInstance<Item>();
        stoneAxe.category = ItemCategory.Tool;
        stoneAxe.itemName = "Stone axe";
        stoneAxe.toolcategory = ToolCategory.Axe;
        stoneAxe.weight = 10;
        stoneAxe.toolEffectiveness = 2.5f;
        stoneAxe.icon = Resources.Load("stone_axe", typeof(Sprite)) as Sprite;
        items[15] = stoneAxe;
        
        Item IronAxe = ScriptableObject.CreateInstance<Item>();
        IronAxe.category = ItemCategory.Tool;
        IronAxe.itemName = "Iron axe";
        IronAxe.toolcategory = ToolCategory.Axe;
        IronAxe.weight = 10;
        IronAxe.toolEffectiveness = 4f;
        IronAxe.icon = Resources.Load("iron_axe", typeof(Sprite)) as Sprite;
        items[16] = IronAxe;

        Item GunAmmo = ScriptableObject.CreateInstance<Item>();
        GunAmmo.category = ItemCategory.Ammo;
        GunAmmo.itemName = "Bullet";
        GunAmmo.weight = 0.01f;
        GunAmmo.isStackable = true;
        GunAmmo.icon = Resources.Load("ammo_icon", typeof(Sprite)) as Sprite;
        items[17] = GunAmmo;

        Item Gun = ScriptableObject.CreateInstance<Item>();
        Gun.category = ItemCategory.Gun;
        Gun.itemName = "Gun";
        Gun.weight = 5;
        Gun.isStackable = false;
        Gun.icon = Resources.Load("gun_icon", typeof(Sprite)) as Sprite;
        items[18] = Gun;
    }
}