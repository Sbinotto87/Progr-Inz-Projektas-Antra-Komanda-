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

    private void Awake()
    {
        // Initialize block array with 4 blocks
        block = new BlockType[4];

        //Stone
        block[0] = new BlockType
        {
            name = "Stone",
            isSolid = true,
            isBreakable = true,
            tool = "Pickaxe",
            hitCount = 1,
            faces = new byte[] { 1, 1, 1, 1, 1, 1 }
        };

        //Grass
        block[1] = new BlockType
        {
            name = "Grass",
            isSolid = true,
            isBreakable = true,
            tool = "Shovel",
            hitCount = 1,
            faces = new byte[] { 0, 0, 0, 0, 2, 0 }
        };

        //Wood
        block[2] = new BlockType
        {
            name = "Wood",
            isSolid = true,
            isBreakable = true,
            tool = "Axe",
            hitCount = 2,
            faces = new byte[] { 3, 3, 3, 3, 3, 3 }
        };

        //Leaves
        block[3] = new BlockType
        {
            name = "Leaves",
            isSolid = true,
            isBreakable = true,
            tool = "Axe",
            hitCount = 1,
            faces = new byte[] { 2, 2, 2, 2, 2, 2 }
        };
    }

        //private void Start()
    //{
        //Mesh combinedMesh = new Mesh();
        //CombineInstance[] combine = new CombineInstance[block.Length];

        //for (int i = 0; i < block.Length; i++)
        //{
        //    Mesh m = Cube.GenerateMesh(new Vector3(i * 1.2f, 0, 0), i);
        //    combine[i].mesh = m;
        //    combine[i].transform = Matrix4x4.identity;
        //}

        //combinedMesh.CombineMeshes(combine, true, false);

        //meshFilter.mesh = combinedMesh;
        //meshRenderer.material = blockMaterial;
        /*
         //meshFilter.mesh = Cube.GenerateMesh(new Vector3(0, 0, 1), 0);
         //for testing
         //creates 3 blocks and combines them together into a single mesh
         Mesh block1 = Cube.GenerateMesh(new Vector3(10, 7, 1), 0);
         Mesh block2 = Cube.GenerateMesh(new Vector3(10, 8, 2), 0);
         Mesh block3 = Cube.GenerateMesh(new Vector3(10, 7, 2), 0);
         Mesh combinedMesh = new Mesh();
         combinedMesh.CombineMeshes(new CombineInstance[] {
             new() {
             mesh = block1,
             transform = meshFilter.transform.localToWorldMatrix },
             new() {
             mesh = block2,
             transform = meshFilter.transform.localToWorldMatrix },
             new() {
             mesh = block3,
             transform = meshFilter.transform.localToWorldMatrix }
         });
         meshFilter.mesh = combinedMesh;
        */
    //}
}

/// <summary>
/// Block types
/// </summary>
public class BlockType
{
    public string name;
    public bool isSolid;
    public bool isBreakable;
    public string tool;
    public int hitCount;
    
    //always set 6 faces
    public byte[] faces;    //front, back, right, left, top, bottom
}
