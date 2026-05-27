using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public Item itemData;
    public int count; 

    public InventorySlot(Item item, int ammount)
    {
        itemData = item;
        count = ammount;
    }
}

public class Inventory : MonoBehaviour
{
    public float maxWeight = 50f;
    public float currentWeight = 0f;
    public System.Action OnInventoryChanged;

    public List<InventorySlot> slots = new List<InventorySlot>();

    public void AddItem(Item newItem)
    {
        if (currentWeight + newItem.weight > maxWeight) return; 

        bool foundStack = false;

        if (newItem.isStackable)
        {
            foreach (InventorySlot slot in slots)
            {
                if (slot.itemData.Equals(newItem))
                {
                    slot.count++;
                    foundStack = true;
                    break;
                }
            }
        }

        if (!foundStack)
        {
            slots.Add(new InventorySlot(newItem, 1));
        }

        currentWeight += newItem.weight;
        OnInventoryChanged?.Invoke();
        //if (currentWeight + newItem.weight <= maxWeight)
        //{
        //    items.Add(newItem);
        //    currentWeight += newItem.weight;

        //    OnInventoryChanged?.Invoke();
        //}
    }

    public void RemoveItem(Item itemToRemove)
    {
        InventorySlot slotToRemove = null;

        foreach (InventorySlot slot in slots)
        {
            if (slot.itemData.Equals(itemToRemove))
            {
                slot.count--;
                currentWeight -= itemToRemove.weight;

                if (slot.count <= 0)
                    slotToRemove = slot;
                break;
            }
        }

        if (slotToRemove != null) slots.Remove(slotToRemove);
        currentWeight = Mathf.Max(0, currentWeight);
        OnInventoryChanged?.Invoke();
    }

    private void Update()
    {

    }
    public void RemoveFullStack(Item itemType)
    {
        InventorySlot slot = slots.Find(s => s.itemData == itemType);
        if (slot != null)
        {
            currentWeight -= (slot.itemData.weight * slot.count);
            slots.Remove(slot);
            currentWeight = Mathf.Max(0, currentWeight);
            OnInventoryChanged?.Invoke();
        }
    }


    // TEMP TEST FOR FOOD ITEM YOU CAN DELETE IF NEEDED I DONT CARE ABOUT THIS METHOD I HATE THIS =============================
    [Header("Test Settings")]
    public Item startingItem;
    public int startingAmount = 5;

    private void Start()
    {
        // Seed the inventory
        SpawnStartingItems();

        // FORCE the UI to update once everything is added
        // This is a safety net in case the UI missed the event during startup
        InventoryUI ui = Object.FindFirstObjectByType<InventoryUI>();
        if (ui != null && ui.inventoryPanel.activeSelf)
        {
            ui.RefreshUI();
        }
    }

    private void SpawnStartingItems()
    {
        CreatedItems createdItems = GameObject.Find("CreatedItems").GetComponent<CreatedItems>();
        startingItem = createdItems.items[11];
        if (startingItem != null)
        {
            AddItem(createdItems.items[6]);
            AddItem(createdItems.items[5]);
            AddItem(createdItems.items[9]);
            AddItem(createdItems.items[18]);    

            // We loop based on the startingAmount
            for (int i = 0; i < startingAmount; i++)
            {
                AddItem(startingItem);
                AddItem(createdItems.items[17]);

            }

            Debug.Log($"Spawned {startingAmount} of {startingItem.itemName} for testing.");
        }
    }
    //=====================================================================================================================================
}
