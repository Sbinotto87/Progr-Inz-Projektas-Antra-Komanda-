using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static UnityEditor.Progress;

public class Inventory : MonoBehaviour
{
    public float maxWeight = 50f;
    public float currentWeight = 0f;
    public System.Action OnInventoryChanged;

    public List<Item> items = new List<Item>();

    public void AddItem(Item newItem)
    {
        if (currentWeight + newItem.weight <= maxWeight)
        {
            items.Add(newItem);
            currentWeight += newItem.weight;

            OnInventoryChanged?.Invoke();
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

            OnInventoryChanged?.Invoke();
        }
    }

    private void Update()
    {

    }
}
