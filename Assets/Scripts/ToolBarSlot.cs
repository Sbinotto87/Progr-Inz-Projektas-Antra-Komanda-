using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Represents a single slot in the toolbar.
/// Attach this to each of the 9 slot GameObjects under your ToolbarPanel.
/// Requires: an Image child named "Icon", a TextMeshProUGUI child named "CountText",
/// and an Image child named "Highlight" (start with alpha = 0).
/// </summary>
public class ToolBarSlot : MonoBehaviour, IDropHandler
{
    // Set automatically by ToolbarUI.Init() — no need to fill in Inspector
    [HideInInspector] public int slotIndex;
    [HideInInspector] public Item assignedItem;

    [Header("Child UI References")]
    [SerializeField] private Image iconImage;            // Child Image — shows item icon
    [SerializeField] private TextMeshProUGUI countText;  // Child TMP — shows stack count
    [SerializeField] private Image highlightBorder;      // Child Image — selection glow/border

    private Inventory playerInventory;

    // ─────────────────────────────────────────────
    // Called once by ToolbarUI after the player is found
    // ─────────────────────────────────────────────
    public void Init(int index, Inventory inventory)
    {
        slotIndex = index;
        playerInventory = inventory;
        ClearSlot();
    }

    // ─────────────────────────────────────────────
    // IDropHandler — fires when a DraggableItem is released over this slot
    // ─────────────────────────────────────────────
    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem draggedItem = eventData.pointerDrag?.GetComponent<DraggableItem>();
        if (draggedItem == null || draggedItem.itemData == null) return;

        // Tell the DraggableItem it was caught by a toolbar slot so OnEndDrag
        // knows to snap back to the inventory instead of triggering consume logic
        draggedItem.droppedOnToolbar = true;

        SetItem(draggedItem.itemData);
    }

    // ─────────────────────────────────────────────
    // Public helpers used by ToolbarUI
    // ─────────────────────────────────────────────
    public void SetItem(Item item)
    {
        assignedItem = item;
        RefreshDisplay();
    }

    public void ClearSlot()
    {
        assignedItem = null;
        RefreshDisplay();
    }

    /// <summary>
    /// Refreshes the icon and stack-count label.
    /// Called on init, on drop, and whenever the inventory changes.
    /// </summary>
    public void RefreshDisplay()
    {
        if (assignedItem == null)
        {
            if (iconImage != null) { iconImage.enabled = false; iconImage.sprite = null; }
            if (countText != null) countText.text = "";
            return;
        }

        // Show icon
        if (iconImage != null)
        {
            iconImage.enabled = assignedItem.icon != null;
            iconImage.sprite = assignedItem.icon;
        }

        // Show stack count from the live inventory data
        if (countText != null && playerInventory != null)
        {
            InventorySlot invSlot = playerInventory.slots.Find(s => s.itemData == assignedItem);
            bool hasMultiple = invSlot != null && assignedItem.isStackable && invSlot.count > 1;
            countText.text = hasMultiple ? $"x{invSlot.count}" : "";
        }
    }

    /// <summary>
    /// Toggles the selection highlight border on or off.
    /// </summary>
    public void SetHighlight(bool active)
    {
        if (highlightBorder != null)
            highlightBorder.enabled = active;
    }
}
