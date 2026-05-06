using UnityEngine;
using UnityEngine.InputSystem;

public class BlockPlacer : MonoBehaviour
{
    public BlockSelector selector;       // assign in inspector
    private Inventory playerInventory;
    private ToolBarUI toolbar;

    private void Start()
    {
        // Find the player inventory
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerInventory = player.GetComponent<Inventory>();

        // Find toolbar
        toolbar = Object.FindFirstObjectByType<ToolBarUI>();
    }

    private void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Item selectedItem = toolbar?.GetSelectedItem();
            if (selectedItem == null) return;

            switch (selectedItem.category)
            {
                case ItemCategory.Block:
                    PlaceBlock(selectedItem);
                    break;

                case ItemCategory.Food:
                    Player player = playerInventory.GetComponent<Player>();
                    if (player != null)
                    {
                        player.AddHunger(selectedItem.hungerRestoreValue);
                        playerInventory.RemoveItem(selectedItem);
                        InventoryUI ui = Object.FindFirstObjectByType<InventoryUI>();
                        if (ui != null) ui.RefreshUI();
                    }
                    break;

                case ItemCategory.Drink:
                    Player playerD = playerInventory.GetComponent<Player>();
                    if (playerD != null)
                    {
                        playerD.AddThirst(selectedItem.hungerRestoreValue);
                        playerInventory.RemoveItem(selectedItem);
                        InventoryUI ui = Object.FindFirstObjectByType<InventoryUI>();
                        if (ui != null) ui.RefreshUI();
                    }
                    break;
            }
        }
    }

    private void PlaceBlock(Item itemToPlace)
    {
        if (selector == null || !selector.hasBlockSelected) return;
        if (playerInventory == null || playerInventory.slots.Count == 0) return;

        // Use first item in inventory (later can select specific slot)
        //Item itemToPlace = toolbar?.GetSelectedItem();

        if (itemToPlace == null || itemToPlace.category != ItemCategory.Block) return;

        Chunk chunk = selector.currentChunk;
        if (chunk == null) return;

        // Place block above selected block
        Vector3Int placeLocal = selector.currentLocalPosition + Vector3Int.RoundToInt(selector.hitNormal);
        int x = placeLocal.x;
        int y = placeLocal.y;
        int z = placeLocal.z;

        // Bounds check
        if (x < 0 || x >= Chunk.Width || y < 0 || y >= Chunk.Height || z < 0 || z >= Chunk.Width)
            return;

        // Place the block
        chunk.blocks[x, y, z] = itemToPlace.blockIndex;
        chunk.UpdateChunk();
        chunk.UpdateNeighborChunks(x, z);

        // Remove item from inventory and update UI
        playerInventory.RemoveItem(itemToPlace);
    }
}