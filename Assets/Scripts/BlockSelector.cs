using UnityEngine;
using TMPro;
using Assets.Scripts;

public class BlockSelector : MonoBehaviour
{ 
    [Header("References")]
    public World world;
    public TMP_Text selectedBlockText;
    public GameObject highlightBox;

    [Header("Settings")]
    public float range = 100f;

    public Chunk currentChunk;
    public Vector3Int currentBlockPosition;
    public Vector3Int currentLocalPosition;
    public bool hasBlockSelected;

    [HideInInspector]
    public Vector3 hitNormal;

    void Update()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        Debug.DrawRay(ray.origin, ray.direction * range, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            hitNormal = hit.normal;
            Vector3 voxelPoint = hit.point - hit.normal * 0.01f;
            Vector3Int worldPos = Vector3Int.FloorToInt(voxelPoint);

            // --- NEW: REDIRECT ENTITY HITS TO THEIR ROOT VOXEL ---
            if (hit.collider.CompareTag("Door block"))
            {
                DoorBlock doorScript = hit.collider.GetComponent<DoorBlock>();
                if (doorScript != null)
                {
                    // Override the raycast coordinate with the door's saved anchor point
                    worldPos = Vector3Int.FloorToInt(doorScript.hingePoint);
                }
            }
            // -----------------------------------------------------

            int chunkX = worldPos.x / Chunk.Width;
            int chunkZ = worldPos.z / Chunk.Width;

            if (chunkX < 0 || chunkZ < 0 ||
                chunkX >= World.WorldSize || chunkZ >= World.WorldSize)
                return;

            Chunk chunk = world.chunks[chunkX, chunkZ];
            if (chunk == null) return;

            int localX = worldPos.x - (chunkX * Chunk.Width);
            int localY = worldPos.y;
            int localZ = worldPos.z - (chunkZ * Chunk.Width);

            if (localX < 0 || localX >= Chunk.Width ||
                localY < 0 || localY >= Chunk.Height ||
                localZ < 0 || localZ >= Chunk.Width)
                return;

            int blockID = chunk.blocks[localX, localY, localZ];

            if (blockID == -1)
            {
                hasBlockSelected = false;
                currentChunk = null;
                selectedBlockText.text = "Facing:\nSelected Block:";
                highlightBox.SetActive(false);
                return;
            }

            currentChunk = chunk;
            currentBlockPosition = worldPos;
            currentLocalPosition = new Vector3Int(localX, localY, localZ);
            hasBlockSelected = true;

            BlockType type = chunk.MyBlocks.block[blockID];

            selectedBlockText.text = $"Facing: X: {worldPos.x} | Y: {worldPos.y} | Z: {worldPos.z}\nSelected Block: {type.name}";

            highlightBox.SetActive(true);
            highlightBox.transform.position = new Vector3(
                worldPos.x + 0.5f,
                worldPos.y + 0.5f,
                worldPos.z + 0.5f
            );
        }
        else
        {
            hasBlockSelected = false;
            currentChunk = null;
            selectedBlockText.text = "Facing:\nSelected Block:";
            highlightBox.SetActive(false);
        }
    }
}