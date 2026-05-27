using System.Linq;
using Assets.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BlockPlacer : MonoBehaviour
{
    public BlockSelector selector;
    private Inventory playerInventory;
    private Player player;
    private ToolBarUI toolBar;

    // IMPORTANT: Assign your Door prefab in the inspector, or ensure there is 
    // a hidden one in the scene named "Door block" just like your chest setup.
    public GameObject doorPrefab;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        this.player = playerObj.GetComponent<Player>();
        if (playerObj != null)
            playerInventory = playerObj.GetComponent<Inventory>();
        toolBar = Object.FindFirstObjectByType<ToolBarUI>();
        doorPrefab = GameObject.FindGameObjectWithTag("Door block");
    }

    private void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (selector != null && selector.hasBlockSelected && selector.currentChunk != null)
            {
                var pos = selector.currentLocalPosition;
                int blockID = selector.currentChunk.blocks[pos.x, pos.y, pos.z];

                // --- CHEST INTERACTION ---
                if (blockID == 12)
                {
                    GameObject[] chestBlocks = GameObject.FindGameObjectsWithTag("Chest block");
                    foreach (GameObject chestBlock in chestBlocks)
                    {
                        if (chestBlock.transform.position.Equals(selector.currentBlockPosition))
                        {
                            chestBlock.GetComponent<ChestBlock>().OpenChest();
                            player.currentOpenedChest = player.HasOpenedChest ? chestBlock : null;
                            break;
                        }
                    }
                }
                // --- DOOR INTERACTION (Assuming ID 20 is Door) ---
                else if (blockID == 20)
                {
                    GameObject[] doors = GameObject.FindGameObjectsWithTag("Door block");
                    foreach (GameObject door in doors)
                    {
                        DoorBlock doorScript = door.GetComponent<DoorBlock>();
                        if (doorScript != null)
                        {
                            // Check against the saved hinge point, NOT the shifted transform
                            if (doorScript.hingePoint == selector.currentBlockPosition ||
                                doorScript.hingePoint == selector.currentBlockPosition + Vector3.down)
                            {
                                doorScript.ToggleDoor();
                                break;
                            }
                        }
                    }
                }
                // --- PLACEMENT ---
                else if (!player.HasOpenedChest && !player.HasOpenedInventory)
                {
                    PlaceBlock();
                }
            }
        }
    }

    private void PlaceBlock()
    {
        if (selector == null || !selector.hasBlockSelected) return;
        if (playerInventory == null || toolBar == null) return;

        Item itemToPlace = toolBar.GetSelectedItem();
        if (itemToPlace == null || itemToPlace.category != ItemCategory.Block) return;

        bool inInventory = playerInventory.slots.Exists(s => s.itemData.Equals(itemToPlace));
        if (!inInventory) return;

        Vector3 worldPos = selector.currentBlockPosition + selector.hitNormal;

        // --- DOOR PLACEMENT LOGIC ---
        if (itemToPlace.blockIndex == 20)
        {
            Vector3 topHalfPos = worldPos + Vector3.up;

            // 1. Check if there is space for the top half of the door
            if (selector.world.GetVoxel(topHalfPos) != -1) return; // Blocked!

            // 2. Determine rotation based on player facing direction
            // Snaps to 0, 90, 180, or 270 degrees
            float yRotation = Mathf.Round(player.transform.eulerAngles.y / 90f) * 90f;

            // 3. Place Voxel Hitboxes for bottom AND top
            selector.world.SetVoxel(worldPos, itemToPlace.blockIndex);
            selector.world.SetVoxel(topHalfPos, itemToPlace.blockIndex);

            // 4. Instantiate visual rotating mesh
            Instantiate(doorPrefab, worldPos, Quaternion.Euler(0, yRotation, 0));
        }
        else
        {
            // Standard block placement
            selector.world.SetVoxel(worldPos, itemToPlace.blockIndex);

            // Handle Chest placement
            if (itemToPlace.blockIndex == 12)
            {
                Instantiate(GameObject.Find("Chest block"), worldPos, Quaternion.identity);
            }
        }

        playerInventory.RemoveItem(itemToPlace);
    }
}