using UnityEngine;
using UnityEngine.InputSystem;

public class BlockPlacer : MonoBehaviour
{
    public BlockSelector selector;       // assign in inspector
    private Inventory playerInventory;

    private void Start()
    {
        // Find the player inventory
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerInventory = player.GetComponent<Inventory>();
    }

    private void Update()
    {
        // Right click places a block
        if (Mouse.current.rightButton.wasPressedThisFrame)
            PlaceBlock();
    }

    private void PlaceBlock()
    {
        if (selector == null || !selector.hasBlockSelected) return;
        if (playerInventory == null || playerInventory.slots.Count == 0) return;

        // Use first item in inventory (later can select specific slot)
        Item itemToPlace = playerInventory.slots[0].itemData;
        if (itemToPlace == null) return;

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