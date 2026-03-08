using UnityEngine;

/// <summary>
/// Block class
/// </summary>
public class Blocks : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    public Block[] blocks;

    private void Start()
    {
        meshFilter.mesh = Cube.GenerateMesh(new Vector3(0, 0, 1), 0);
        //for testing
        //creates 3 blocks and combines them together into a single mesh
        /*Mesh block1 = Cube.GenerateMesh(new Vector3(0, 0, 1), 0);
        Mesh block2 = Cube.GenerateMesh(new Vector3(1, 0, 0), 0);
        Mesh block3 = Cube.GenerateMesh(new Vector3(0, 1, 0), 0);
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
        meshFilter.mesh = Cube.GenerateMesh(new Vector3(0, 0, 0), 0);*/
    }
}

/// <summary>
/// Block types
/// </summary>
[System.Serializable]
public class Block
{
    public string name;
    public bool isSolid;
    public bool isBreakable;
    public string tool;
    public int hitCount;
    
    //always set 6 faces
    public byte[] faces;    //front, back, right, left, top, bottom
}
