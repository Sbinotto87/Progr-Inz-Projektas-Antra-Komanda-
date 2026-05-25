using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Manages the 9-slot toolbar at the bottom of the screen.
/// Always visible — lives outside the inventory panel so it never gets hidden.
///
/// Setup:
///   1. Create a new Panel in your "UI elements" canvas (sibling of InventoryUImanager, NOT a child).
///   2. Anchor it to bottom-center.
///   3. Add 9 slot child GameObjects, each with a ToolbarSlot component.
///   4. Drag those 9 slots into the Slots array in this component's Inspector.
/// </summary>
public class ToolBarUI : MonoBehaviour
{
    [Header("Toolbar Slots (assign all 9 in Inspector)")]
    public ToolBarSlot[] slots = new ToolBarSlot[9];

    private int selectedSlotIndex = 0;
    private Inventory playerInventory;

    // ─────────────────────────────────────────────
    // Lifecycle
    // ─────────────────────────────────────────────
    private void Start()
    {
        FindAndBindInventory();
    }

    private void OnDestroy()
    {
        // Clean up delegate to avoid ghost subscriptions after scene reload
        if (playerInventory != null)
            playerInventory.OnInventoryChanged -= RefreshAllSlots;
    }

    // ─────────────────────────────────────────────
    // Per-frame: number keys 1–9
    // Uses the new Input System's Keyboard device directly,
    // so no action map changes are needed.
    // ─────────────────────────────────────────────
    private void Update()
    {
        // Retry finding the player if it wasn't ready at Start (e.g. spawn delay)
        if (playerInventory == null)
        {
            FindAndBindInventory();
            return;
        }

        HandleNumberKeys();
        HandleUseInput();
    }

    // ─────────────────────────────────────────────
    // Input
    // ─────────────────────────────────────────────
    private void HandleNumberKeys()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        // Map digit keys directly — avoids needing new Input Actions
        if (kb.digit1Key.wasPressedThisFrame) SelectSlot(0);
        else if (kb.digit2Key.wasPressedThisFrame) SelectSlot(1);
        else if (kb.digit3Key.wasPressedThisFrame) SelectSlot(2);
        else if (kb.digit4Key.wasPressedThisFrame) SelectSlot(3);
        else if (kb.digit5Key.wasPressedThisFrame) SelectSlot(4);
        else if (kb.digit6Key.wasPressedThisFrame) SelectSlot(5);
        else if (kb.digit7Key.wasPressedThisFrame) SelectSlot(6);
        else if (kb.digit8Key.wasPressedThisFrame) SelectSlot(7);
        else if (kb.digit9Key.wasPressedThisFrame) SelectSlot(8);

    }

    private void HandleUseInput()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        // Right-click uses the selected toolbar item.
        // Skip clicks over UI so managing the inventory doesn't trigger a world action.
        if (mouse.rightButton.wasPressedThisFrame &&
            !EventSystem.current.IsPointerOverGameObject())
        {
            UseSelectedItem();
        }
    }

    // ─────────────────────────────────────────────
    // Selection
    // ─────────────────────────────────────────────
    public void SelectSlot(int index)
    {
        if (index < 0 || index >= slots.Length) return;
        if (slots[selectedSlotIndex] != null) slots[selectedSlotIndex].SetHighlight(false);

        selectedSlotIndex = index;

        if (slots[selectedSlotIndex] != null) slots[selectedSlotIndex].SetHighlight(true);

        UpdateHeldTool();

        Debug.Log($"[Toolbar] Slot {index + 1} selected — " +
                  $"{(slots[index]?.assignedItem != null ? slots[index].assignedItem.itemName : "empty")}");
    }

    // ─────────────────────────────────────────────
    // Public accessors (use these from other scripts to query what's selected)
    // ─────────────────────────────────────────────

    /// <summary>Returns the Item in the currently selected toolbar slot, or null if empty.</summary>
    public Item GetSelectedItem()
    {
        if (slots == null || selectedSlotIndex >= slots.Length) return null;
        return slots[selectedSlotIndex]?.assignedItem;
    }

    public int GetSelectedSlotIndex() => selectedSlotIndex;

    // ─────────────────────────────────────────────
    // Inventory sync
    // ─────────────────────────────────────────────

    /// <summary>
    /// Subscribed to Inventory.OnInventoryChanged.
    /// Clears any toolbar slots whose item was fully consumed,
    /// and refreshes counts on the rest.
    /// </summary>
    public void RefreshAllSlots()
    {
        if (playerInventory == null) return;

        foreach (ToolBarSlot slot in slots)
        {
            if (slot == null || slot.assignedItem == null) continue;

            bool stillInInventory = playerInventory.slots.Exists(s => s.itemData == slot.assignedItem);

            if (!stillInInventory)
                slot.ClearSlot();          // Item was fully consumed or dropped — remove from toolbar
            else
                slot.RefreshDisplay();     // Update the stack count label
        }
    }

    // ─────────────────────────────────────────────
    // Initialisation
    // ─────────────────────────────────────────────
    private void FindAndBindInventory()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        playerInventory = player.GetComponent<Inventory>();
        if (playerInventory == null) return;

        // Pass inventory reference to every slot so they can read stack counts
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
                slots[i].Init(i, playerInventory);
        }

        // Subscribe so toolbar auto-updates when items are consumed or added
        playerInventory.OnInventoryChanged += RefreshAllSlots;

        // Highlight slot 0 by default
        SelectSlot(0);
    }

    private void UpdateHeldTool()
    {
        if (playerInventory == null) return;
        Player player = playerInventory.GetComponent<Player>();
        Transform tool = GameObject.Find("UI elements").transform.Find("tool");
        Image toolImage = tool != null ? tool.GetComponent<Image>() : null;

        Item item = GetSelectedItem();
        bool holdable = item != null &&
                        (item.category == ItemCategory.Tool || item.category == ItemCategory.Weapon);

        if (holdable)
        {
            if (toolImage != null) { toolImage.sprite = item.icon; tool.gameObject.SetActive(true); }
            if (player != null) { player.currentEquippedTool = item; player.HasEquippedTool = true; }
        }
        else
        {
            if (tool != null) tool.gameObject.SetActive(false);
            if (player != null) { player.currentEquippedTool = null; player.HasEquippedTool = false; }
        }
    }
    public void UseSelectedItem()
    {
        Item item = GetSelectedItem();
        if (item == null || playerInventory == null) return;
        Player player = playerInventory.GetComponent<Player>();

        switch (item.category)
        {
            case ItemCategory.Food:
            case ItemCategory.Drink:
                if (player != null)
                {
                    player.AddHealth(item.healthRestoreValue);
                    if (item.category == ItemCategory.Food) player.AddHunger(item.hungerRestoreValue);
                    else player.AddThirst(item.thirstRestoreValue);
                }
                playerInventory.RemoveItem(item);   // fires OnInventoryChanged → inventory + toolbar refresh
                break;

            case ItemCategory.Block:
                // HOOK: this is where block-placement code should read the index.
                // int idx = item.blockIndex;  → hand to whatever places blocks.
                break;
        }
    }
}
