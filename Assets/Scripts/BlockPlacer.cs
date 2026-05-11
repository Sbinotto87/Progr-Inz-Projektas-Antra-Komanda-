using System.Linq;
using Assets.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;

public class BlockPlacer : MonoBehaviour
{
    public BlockSelector selector;       // assign in inspector
    private Inventory playerInventory;
    private Player player;

    private void Start()
    {
        // Find the player inventory
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        this.player = player.GetComponent<Player>();
        if (player != null)
            playerInventory = player.GetComponent<Inventory>();
    }

    private void Update()
    {
        // Right click places a block
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (selector != null && selector.hasBlockSelected && selector.currentChunk != null)
            {
                var pos = selector.currentLocalPosition;
                int blockID = selector.currentChunk.blocks[pos.x, pos.y, pos.z];
                if (blockID == 12)
                {
                    GameObject[] chestBlocks = GameObject.FindGameObjectsWithTag("Chest block");
                    foreach (GameObject chestBlock in chestBlocks)
                    {
                        if (chestBlock.transform.position.Equals(new Vector3(pos.x, pos.y, pos.z)))
                        {
                            chestBlock.GetComponent<ChestBlock>().OpenChest();
                            if (player.HasOpenedChest)
                                player.currentOpenedChest = chestBlock;
                            else player.currentOpenedChest = null;
                            
                            break;
                        }
                    }
                }
                else if (!player.HasOpenedChest && !player.HasOpenedInventory) PlaceBlock();
            }
        }
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
        
        if (itemToPlace.blockIndex == 12)
        {
            GameObject chest = Instantiate(GameObject.Find("Chest block"), new Vector3(x, y, z), Quaternion.identity);
            //GameObject chest = new GameObject("Chest block");
            //chest.AddComponent<ChestBlock>();
            //chest.transform.position = new Vector3(x, y, z);
        }
    }

    private void OpenChest()
    {

    }
}