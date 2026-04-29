using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class CraftingItem
{
    public List<KeyValuePair<Item, int>> recipe;     //recipe contains pairs of items and their amounts
    public Item result;      //result is item object
}

public class CraftingItems : MonoBehaviour
{
    public CraftingItem[] craftingItems;
    private Item[] items;
    
    private void Awake()
    {
        craftingItems = new CraftingItem[3];
        GameObject createdItems = GameObject.Find("CreatedItems");
        if (createdItems == null)
            createdItems = new GameObject("CreatedItems", typeof(CreatedItems));
        CreatedItems createdItems1 = createdItems.GetComponent<CreatedItems>();
        items = createdItems1.items;

        craftingItems[0] = new CraftingItem()
        {
            recipe = new()
            {
                new KeyValuePair<Item, int>(items[2], 2),
                new KeyValuePair<Item, int>(items[0], 3)
            },
            result = items[7]
        };

        craftingItems[1] = new CraftingItem()
        {
            recipe = new()
            {
                new KeyValuePair<Item, int>(items[8], 3),
                new KeyValuePair<Item, int>(items[2], 2)
            },
            result = items[5]
        };
        
        craftingItems[2] = new CraftingItem()
        {
            recipe = new()
            {
                new KeyValuePair<Item, int>(items[8], 2),
                new KeyValuePair<Item, int>(items[2], 2)
            },
            result = items[6]
        };
    }
}