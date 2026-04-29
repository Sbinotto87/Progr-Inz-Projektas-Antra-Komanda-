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


    public Item stoneItem;
    public Item grassBlockItem;
    public Item woodItem;
    public Item leafItem;
    public Item grassItem;
    public Item glassItem;
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
        block = new BlockType[12];
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
            tool = "Pickaxe",
            hitCount = 1,
            faces = new byte[] { 1, 1, 1, 1, 1, 1 },
            dropItem = items[0],
            breakSound = stoneBreakSound
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
            tool = "Shovel",
            hitCount = 1,
            faces = new byte[] { 0, 0, 0, 0, 2, 0 },
            dropItem = items[1],
            breakSound = grassBreakSound
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
            tool = "Axe",
            hitCount = 2,
            faces = new byte[] { 3, 3, 3, 3, 3, 3 },
            dropItem = items[2],
            breakSound = woodBreakSound
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
            tool = "Axe",
            hitCount = 1,
            faces = new byte[] { 2, 2, 2, 2, 2, 2 },
            dropItem = items[3],
            breakSound = leafBreakSound
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
            tool = "Axe",
            hitCount = 1,
            faces = new byte[] { 4, 4, 4, 4, 15, 15 },
            dropItem = items[4],
            breakSound = grassBreakSound
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
            tool = "Axe",
            hitCount = 2,
            faces = new byte[] { 2, 2, 2, 2, 2, 2 },
            dropItem = defaultItem,
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
            tool = "Pickaxe",
            hitCount = 3,
            faces = new byte[] { 6, 6, 6, 6, 6, 6 },
            dropItem = defaultItem
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
            tool = "Pickaxe",
            hitCount = 3,
            faces = new byte[] { 7, 7, 7, 7, 7, 7 },
            dropItem = defaultItem
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
            tool = "pickaxe",
            hitCount = 3,
            faces = new byte[] { 8, 8, 8, 8, 8, 8 },
            dropItem = defaultItem
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
            tool = "NA",
            hitCount = 99,
            faces = new byte[] { 0, 0, 0, 0, 0, 0 },
            dropItem = defaultItem
        };
        
        block[10] = new BlockType
        {
            name = "Iron ore",
            isSolid = true,
            isTransparent = false,
            isBreakable = true,
            isCutout = false,
            isSwimable = false,
            tool = "pickaxe",
            hitCount = 3,
            faces = new byte[] { 8, 8, 8, 8, 8, 8 },
            dropItem = items[8]
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
            tool = "NA",
            hitCount = 99,
            faces = new byte[] { 1, 1, 1, 1, 1, 1 },
            dropItem = null
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
    public string tool;
    public int hitCount;
    
    //always set 6 faces
    public byte[] faces;    //front, back, right, left, top, bottom
    public Item dropItem;

    public AudioClip breakSound; // for breaking sounds
}
