using UnityEngine;
using UnityEngine.InputSystem;

public class BlockBreaking : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    public BlockSelector selector;

    void Update()
    {

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (selector.hasBlockSelected && selector.currentChunk != null)
            {
                var pos = selector.currentLocalPosition;

                selector.currentChunk.blocks[pos.x, pos.y, pos.z] = -1;

                selector.currentChunk.UpdateChunk();
                selector.currentChunk.UpdateNeighborChunks(selector.currentLocalPosition.x, selector.currentLocalPosition.z);
            }
        }
    }
}
