using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CraftingUI : MonoBehaviour
{
    private Inventory inventory;
    public GameObject craftingPanel;
    public GameObject slot;
    public Transform listParent;
    private PlayerInput playerInput;
    private CraftingItems craftingitemsClass;
    private bool isOpen;
    
    void Start()
    {
        craftingPanel.SetActive(false);
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        inventory = player.GetComponent<Inventory>();
        playerInput = player.GetComponent<PlayerInput>();
        craftingitemsClass = craftingPanel.GetComponent<CraftingItems>();
    }

    void Update()
    {
        if (playerInput.actions["Inventory"].WasPressedThisFrame())
            ToggleCraftingList();
    }

    public void ToggleCraftingList()
    {
        isOpen = !isOpen;
        craftingPanel.SetActive(isOpen);

        if (isOpen)
            RefreshCraftingUI();
    }
    
    public void RefreshCraftingUI()
    {
        foreach (Transform child in listParent)
            Destroy(child.gameObject);
        
        bool isItemValid;
        foreach (CraftingItem craftingitem in craftingitemsClass.craftingItems)
        {
            isItemValid = true;
            bool isValid;
            foreach (KeyValuePair<Item, int> recipeItem in craftingitem.recipe)
            {
                isValid = false;
                foreach (InventorySlot item in inventory.slots)
                {
                    if (recipeItem.Key.Equals(item.itemData) && recipeItem.Value <= item.count)
                        isValid = true;
                }
                if (!isValid) isItemValid = false;
            }

            if (!isItemValid) continue;
            
            GameObject newslot = Instantiate(slot, listParent);
            var text = newslot.GetComponentInChildren<TextMeshProUGUI>();
            text.text = $"{craftingitem.result.itemName}";
        }
    }
    
    public void OnClickButton(GameObject button)
    {
        craftingitemsClass = button.GetComponent<CraftingItems>();
        inventory = GameObject.FindGameObjectWithTag("Player").GetComponent<Inventory>();
        listParent = GameObject.Find("Crafting UI manager").transform.Find("CraftingPanel/Viewport/Content") .GetComponent<Transform>();
        
        var text = button.GetComponentInChildren<TextMeshProUGUI>();
        foreach (CraftingItem craftingitem in craftingitemsClass.craftingItems)
            if (craftingitem.result.itemName.Equals(text.text))
            {
                foreach (KeyValuePair<Item, int> recipeItem in craftingitem.recipe)
                    for (int i = 0; i < recipeItem.Value; i++)
                        inventory.RemoveItem(recipeItem.Key);
                inventory.AddItem(craftingitem.result);
                break;
            }
        RefreshCraftingUI();
    }
}