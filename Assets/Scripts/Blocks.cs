using UnityEngine;

/// <summary>
/// Block class
/// </summary>
public class Blocks : MonoBehaviour
{
    [Header("Block setup")]
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    public Material blockMaterial;

    public BlockType[] block;
    private Item[] items;

    [Header("Break Sounds")] 
    public AudioClip stoneBreakSound;
    public AudioClip grassBreakSound;
    public AudioClip woodBreakSound;
    public AudioClip leafBreakSound;
    public AudioClip glassBreakSound;
    public AudioClip metalBreakSound;
    public AudioClip brickBreakSound;
    
    public Item defaultItem;

    [Header("Current block")]
    public int blockIndex;

    public BlockType CurrentBlockType
    {
        get
        {
            if (blockIndex >= 0 && blockIndex < block.Length)
                return block[blockIndex];

            return null;
        }
    }

    private void Awake()
    {
        block = new BlockType[21];
        GameObject createdItems = GameObject.Find("CreatedItems");
        if (createdItems == null)
            createdItems = new GameObject("CreatedItems", typeof(CreatedItems));
        CreatedItems createdItems1 = createdItems.GetComponent<CreatedItems>();
        items = createdItems1.items;
        
        //Stone
        block[0] = new BlockType
        {
            name = "Stone block",
            isSolid = true,
            isTransparent = false,
            isBreakable = true,
            isCutout = false,
            isSwimable = false,
            swimSlowdown = 1.0f,
            tool = ToolCategory.Pickaxe,
            hitCount = 15,
            faces = new byte[] { 1, 1, 1, 1, 1, 1 },
            dropItem = items[0],
            breakSound = stoneBreakSound,
            mesh = MeshType.Full
        };

        //Grass (block)
        block[1] = new BlockType
        {
            name = "Grass block",
            isSolid = true,
            isTransparent = false,
            isBreakable = true,
            isCutout = false,
            isSwimable = false,
            swimSlowdown = 1.0f,
            tool = ToolCategory.Shovel,
            hitCount = 4,
            faces = new byte[] { 0, 0, 0, 0, 2, 0 },
            dropItem = items[1],
            breakSound = grassBreakSound,
            mesh = MeshType.Full
        };

        //Wood
        block[2] = new BlockType
        {
            name = "Wood block",
            isSolid = true,
            isTransparent = false,
            isBreakable = true,
            isCutout = false,
            isSwimable = false,
            swimSlowdown = 1.0f,
            tool = ToolCategory.Axe,
            hitCount = 5,
            faces = new byte[] { 3, 3, 3, 3, 3, 3 },
            dropItem = items[2],
            breakSound = woodBreakSound,
            mesh = MeshType.Full
        };

        //Leaves
        block[3] = new BlockType
        {
            name = "Leaf block",
            isSolid = true,
            isTransparent = false,
            isBreakable = true,
            isCutout = false,
            isSwimable = false,
            swimSlowdown = 1.0f,
            tool = ToolCategory.Axe,
            hitCount = 1,
            faces = new byte[] { 2, 2, 2, 2, 2, 2 },
            dropItem = items[3],
            breakSound = leafBreakSound,
            mesh = MeshType.Full
        };
        
        //Grass
        block[4] = new BlockType
        {
            name = "Grass",
            isSolid = false,
            isTransparent = false,
            isBreakable = true,
            isCutout = true,
            isSwimable = false,
            swimSlowdown = 1.0f,
            tool = ToolCategory.Shovel,
            hitCount = 1,
            faces = new byte[] { 4, 4, 4, 4, 15, 15 },
            dropItem = items[4],
            breakSound = grassBreakSound,
            mesh = MeshType.Grass
        };
        
        //Glass
        block[5] = new BlockType
        {
            name = "Glass",
            isSolid = true,
            isTransparent = true,
            isBreakable = true,
            isCutout = false,
            isSwimable = false,
            swimSlowdown = 1.0f,
            tool = ToolCategory.NA,
            hitCount = 2,
            faces = new byte[] { 5, 5, 5, 5, 5, 5 },
            dropItem = defaultItem,
            breakSound = glassBreakSound,
            mesh = MeshType.Full
        };
        
        block[6] = new BlockType
        {
            name = "Bricks",
            isSolid = true,
            isTransparent = false,
            isBreakable = true,
            isCutout = false,
            isSwimable = false,
            swimSlowdown = 1.0f,
            tool = ToolCategory.Pickaxe,
            hitCount = 15,
            faces = new byte[] { 6, 6, 6, 6, 6, 6 },
            dropItem = defaultItem,
            breakSound = brickBreakSound,
            mesh = MeshType.Full
        };
        
        block[7] = new BlockType
        {
            name = "Copper",
            isSolid = true,
            isTransparent = false,
            isBreakable = true,
            isCutout = false,
            isSwimable = false,
            swimSlowdown = 1.0f,
            tool = ToolCategory.Pickaxe,
            hitCount = 15,
            faces = new byte[] { 7, 7, 7, 7, 7, 7 },
            dropItem = defaultItem,
            breakSound = metalBreakSound,
            mesh = MeshType.Full
        };
        
        block[8] = new BlockType
        {
            name = "Copper bricks",
            isSolid = true,
            isTransparent = false,
            isBreakable = true,
            isCutout = false,
            isSwimable = false,
            swimSlowdown = 1.0f,
            tool = ToolCategory.Pickaxe,
            hitCount = 15,
            faces = new byte[] { 8, 8, 8, 8, 8, 8 },
            dropItem = defaultItem,
            breakSound = metalBreakSound,
            mesh = MeshType.Full
        };
        
        block[9] = new BlockType
        {
            name = "Water",
            isSolid = false,
            isTransparent = true,
            isBreakable = false,
            isCutout = false,
            isSwimable = true,
            swimSlowdown = 2.0f,
            tool = ToolCategory.NA,
            hitCount = 99,
            faces = new byte[] { 0, 0, 0, 0, 0, 0 },
            dropItem = defaultItem,
            mesh = MeshType.Full
        };
        
        block[10] = new BlockType
        {
            name = "Iron ore",
            isSolid = true,
            isTransparent = false,
            isBreakable = true,
            isCutout = false,
            isSwimable = false,
            tool = ToolCategory.Pickaxe,
            hitCount = 15,
            faces = new byte[] { 8, 8, 8, 8, 8, 8 },
            dropItem = defaultItem,
            breakSound = metalBreakSound,
            mesh = MeshType.Full
        };
        
        block[11] = new BlockType
        {
            name = "Oil",
            isSolid = false,
            isTransparent = true,
            isBreakable = false,
            isCutout = false,
            isSwimable = true,
            swimSlowdown = 1.0f,
            tool = ToolCategory.NA,
            hitCount = 99,
            faces = new byte[] { 1, 1, 1, 1, 1, 1 },
            dropItem = null,
            mesh = MeshType.Full
        };
        
        block[12] = new BlockType
        {
            name = "Chest block",
            isSolid = true,
            isTransparent = false,
            isBreakable = true,
            isCutout = false,
            isSwimable = false,
            tool = ToolCategory.Axe,
            hitCount = 5,
            faces = new byte[] { 1, 1, 1, 1, 1, 1 },
            dropItem = items[9],
            mesh = MeshType.Full
        };
        block[13] = new BlockType
        {
            name = "Oil0875",
            isSolid = false,
            isTransparent = true,
            isBreakable = false,
            isCutout = false,
            isSwimable = true,
            swimSlowdown = 1.0f,
            tool = ToolCategory.NA,
            hitCount = 99,
            faces = new byte[] { 1, 1, 1, 1, 1, 1 },
            dropItem = defaultItem,
            mesh = MeshType.Nf0875
        };
        block[14] = new BlockType
        {
            name = "Oil075",
            isSolid = false,
            isTransparent = true,
            isBreakable = false,
            isCutout = false,
            isSwimable = true,
            swimSlowdown = 1.0f,
            tool = ToolCategory.NA,
            hitCount = 99,
            faces = new byte[] { 1, 1, 1, 1, 1, 1 },
            dropItem = defaultItem,
            mesh = MeshType.Nf075
        };
        block[15] = new BlockType
        {
            name = "Oil0625",
            isSolid = false,
            isTransparent = true,
            isBreakable = false,
            isCutout = false,
            isSwimable = true,
            swimSlowdown = 1.0f,
            tool = ToolCategory.NA,
            hitCount = 99,
            faces = new byte[] { 1, 1, 1, 1, 1, 1 },
            dropItem = defaultItem,
            mesh = MeshType.Nf0625
        };
        block[16] = new BlockType
        {
            name = "Oil05",
            isSolid = false,
            isTransparent = true,
            isBreakable = false,
            isCutout = false,
            isSwimable = true,
            swimSlowdown = 1.0f,
            tool = ToolCategory.NA,
            hitCount = 99,
            faces = new byte[] { 1, 1, 1, 1, 1, 1 },
            dropItem = defaultItem,
            mesh = MeshType.Nf05
        };
        block[17] = new BlockType
        {
            name = "Oil0375",
            isSolid = false,
            isTransparent = true,
            isBreakable = false,
            isCutout = false,
            isSwimable = true,
            swimSlowdown = 1.0f,
            tool = ToolCategory.NA,
            hitCount = 99,
            faces = new byte[] { 1, 1, 1, 1, 1, 1 },
            dropItem = defaultItem,
            mesh = MeshType.Nf0375
        };
        block[18] = new BlockType
        {
            name = "Oil025",
            isSolid = false,
            isTransparent = true,
            isBreakable = false,
            isCutout = false,
            isSwimable = true,
            swimSlowdown = 1.0f,
            tool = ToolCategory.NA,
            hitCount = 99,
            faces = new byte[] { 1, 1, 1, 1, 1, 1 },
            dropItem = defaultItem,
            mesh = MeshType.Nf025
        };
        block[19] = new BlockType
        {
            name = "Oil0125",
            isSolid = false,
            isTransparent = true,
            isBreakable = false,
            isCutout = false,
            isSwimable = true,
            swimSlowdown = 1.0f,
            tool = ToolCategory.NA,
            hitCount = 99,
            faces = new byte[] { 1, 1, 1, 1, 1, 1 },
            dropItem = defaultItem,
            mesh = MeshType.Nf0125
        };
        block[20] = new BlockType
        {
            name = "Door block",
            isSolid = false,
            isTransparent = false, // Must be true so we don't cull the blocks behind it
            isBreakable = true,
            isCutout = false,
            isSwimable = false,
            tool = ToolCategory.Axe,
            hitCount = 3,
            faces = new byte[] { 15, 15, 15, 15, 15, 15 }, // Just use glass texture ID or similar
            dropItem = items[17], // Drops the door item we created earlier
            mesh = MeshType.Full // Or a custom invisible MeshType if you want the GameObject to be the ONLY visual
        };
    }
    
}

/// <summary>
/// Block types
/// </summary>
public class BlockType
{
    public string name;
    public bool isSolid;
    public bool isTransparent;
    public bool isCutout; //for non see-through like grass
    public bool isSwimable;
    public float swimSlowdown;
    public bool isBreakable;
    public ToolCategory tool;
    public int hitCount;
    public MeshType mesh;

    //always set 6 faces
    public byte[] faces;    //front, back, right, left, top, bottom
    public Item dropItem;

    public AudioClip breakSound; // for breaking sounds
    public MeshType GetMeshType(int id)
    {
        return this.mesh;
    }
}

public enum MeshType
{
    Full,
    Grass,
    Nf0875,//Not full and % of how much not full from top, ie Nf05 is half a block tall, like a slab
    Nf075,
    Nf0625,
    Nf05,
    Nf0375,
    Nf025,
    Nf0125,
    DoorZ, 
    DoorX
}

public enum ToolCategory
{
    Pickaxe,
    Axe,
    Shovel,
    NA
}
