using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts
{
    public class ChestBlock : MonoBehaviour
    {
        private readonly int maxSlotsCount = 10;
        
        public GameObject panel;
        public GameObject slot;
        public Transform listParent;
        private PlayerInput playerInput;
        
        public List<InventorySlot> chestItemsList;
        private bool isOpen;
        private int currSlotsCount;

        void Start()
        {
            chestItemsList = new List<InventorySlot>();
            panel.SetActive(false);
            playerInput = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInput>();
            isOpen = false;
            currSlotsCount = 0;
        }
        
        public void OpenChest()
        {
            isOpen = !isOpen;
            panel.SetActive(isOpen);
            GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().HasOpenedChest = isOpen;

            if (isOpen)
            {
                RefreshUI();
                if (playerInput != null)
                    playerInput.SwitchCurrentActionMap("Settings");

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                if (playerInput != null)
                    playerInput.SwitchCurrentActionMap("Player");

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        public void RefreshUI()
        {
            foreach (Transform child in listParent)
            {
                Destroy(child.gameObject);
            }

            foreach (InventorySlot item in chestItemsList)
            {
                GameObject newSlot = Instantiate(slot, listParent);

                var text = newSlot.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    string countText = item.itemData.isStackable ? $" x{item.count}" : "";
                    text.text = $"{item.itemData.itemName}{countText} ({item.itemData.weight * item.count} kg)";
                }

                DraggableItem dragScript = newSlot.GetComponent<DraggableItem>();
                if (dragScript != null)
                {
                    dragScript.itemData = item.itemData;
                }
            }
        }

        /// <summary>
        /// Adds one item to the chest
        /// </summary>
        /// <param name="item">item to add</param>
        /// <returns>1 if new slot was created, 0 otherwise</returns>
        public int addItem(Item item)
        {
            bool foundStack = false;
            if (item.isStackable)
            {
                foreach (InventorySlot slot in chestItemsList)
                {
                    if (slot.itemData.Equals(item))
                    {
                        slot.count++;
                        foundStack = true;
                        break;
                    }
                }
            }

            if (!foundStack)
            {
                if (currSlotsCount < maxSlotsCount)
                {
                    chestItemsList.Add(new InventorySlot(item, 1));
                    currSlotsCount++;
                    return 1;
                }
            }

            return 0;
        }
        
        /// <summary>
        /// Removes one item from the chest
        /// </summary>
        /// <param name="item">item to remove</param>
        /// <returns>1 if slot was removed, 0 otherwise</returns>
        public int removeItem(Item item)
        {
            InventorySlot slotToRemove = null;

            foreach (InventorySlot slot in chestItemsList)
            {
                if (slot.itemData.Equals(item))
                {
                    slot.count--;

                    if (slot.count <= 0)
                        slotToRemove = slot;
                    break;
                }
            }

            if (slotToRemove != null)
            {
                chestItemsList.Remove(slotToRemove);
                return 1;
            }

            return 0;
        }
    }
}