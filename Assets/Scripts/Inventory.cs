using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public float maxWeight = 50f;
    public float currentWeight = 0f;
    public InventoryUI uiDisplay;

    public List<Item> items = new List<Item>();

    public void AddItem(Item newItem)
    {
        if (currentWeight + newItem.weight <= maxWeight)
        {
            items.Add(newItem);
            currentWeight += newItem.weight;

            if (uiDisplay != null) uiDisplay.RefreshUI(); 
        }
    }

    public void RemoveItem(Item itemToRemove)
    {
        if (items.Contains(itemToRemove))
        {
            items.Remove(itemToRemove);
            currentWeight -= itemToRemove.weight;

            // Clamp weight so it doesn't go below 0 due to float math errors
            currentWeight = Mathf.Max(0, currentWeight);

            if (uiDisplay != null) uiDisplay.RefreshUI();
        }
    }

    // Test funcionality

    private float timer = 0f;
    public float interval = 5f; 
    public Item timeGiftItem;   
    private void Update()
    {
        timer += Time.deltaTime; 

        if (timer >= interval)
        {
            AddItem(timeGiftItem);

            timer = 0f;
        }
    }
}
