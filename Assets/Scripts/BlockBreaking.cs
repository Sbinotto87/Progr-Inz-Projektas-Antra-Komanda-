using Assets.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;

public class BlockBreaking : MonoBehaviour
{
    public BlockSelector selector;
    private Inventory playerInventory;
    private AudioSource audioSource; // for block breaking sounds
    private Player player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        this.player = player.GetComponent<Player>();
        playerInventory = player.GetComponent<Inventory>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && !player.HasOpenedInventory && !player.HasOpenedChest)
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

                    if (type.breakSound != null && audioSource != null)
                    {
                        audioSource.Stop();
                        audioSource.clip = type.breakSound;
                        audioSource.Play();
                        StartCoroutine(StopSoundAfter(0.3f)); // 0.3 sekundės
                    }
                }

                // Remove block
                if (selector.currentChunk.blocks[pos.x, pos.y, pos.z] == 12)
                {
                    GameObject[] chestBlocks = GameObject.FindGameObjectsWithTag("Chest block");
                    foreach (GameObject chestBlock in chestBlocks)
                    {
                        if (chestBlock.transform.position.Equals(new Vector3(pos.x, pos.y, pos.z)))
                        {
                            Destroy(chestBlock);
                            break;
                        }
                    }
                }

                //selector.currentChunk.blocks[pos.x, pos.y, pos.z] = -1;

                //selector.currentChunk.UpdateChunk();
                //selector.currentChunk.UpdateNeighborChunks(pos.x, pos.z);
                Vector3 worldPos = selector.currentBlockPosition;
                selector.world.SetVoxel(worldPos, -1);
            }
        }
    }

    private System.Collections.IEnumerator StopSoundAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.Stop();
    }
}
