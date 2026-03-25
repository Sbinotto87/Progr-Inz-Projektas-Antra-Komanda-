using UnityEngine;
using UnityEngine.InputSystem;

public class BlockBreaking : MonoBehaviour
{
    public BlockSelector selector;
    private Inventory playerInventory;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerInventory = player.GetComponent<Inventory>();
    }

    // Update is called once per frame

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (selector.hasBlockSelected && selector.currentChunk != null)
            {
                var pos = selector.currentLocalPosition;

                int blockID = selector.currentChunk.blocks[pos.x, pos.y, pos.z];

                if (blockID != -1)
                {
                    BlockType type = selector.currentChunk.MyBlocks.block[blockID];

                    if (type.dropItem != null && playerInventory != null)
                    {
                        playerInventory.AddItem(type.dropItem);
                    }
                }

                // Remove block
                selector.currentChunk.blocks[pos.x, pos.y, pos.z] = -1;

                selector.currentChunk.UpdateChunk();
                selector.currentChunk.UpdateNeighborChunks(pos.x, pos.z);
            }
        }
    }
}
