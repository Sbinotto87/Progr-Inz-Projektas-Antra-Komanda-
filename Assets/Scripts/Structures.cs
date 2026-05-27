using System;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using Random = UnityEngine.Random;

public class Structures
{
    private static readonly double TreeDensity = 0.01;
    private static readonly double GrassDensity = 0.05;
    private static readonly double BuildingsDensity = 0.007;
    private static readonly int mallX = 50, mallY = 8, mallZ = 25, mallOffset = 0;  //Do not change values without consulting

    
    public static void GenerateOres(Chunk chunk, int blockType)
    {
        int oresInChunk = Random.Range(2, 5);
        for (int i = 0; i < oresInChunk; i++)
        {
            int length = Random.Range(2, 5);
            int height = Random.Range(3, 6);
            int width = Random.Range(2, 5);
            int x = Random.Range(length, Chunk.Width - length - 1);
            int z = Random.Range(width, Chunk.Width - width - 1);
            int y = Random.Range(height, Chunk.Height / 3);
            for (int j = x - length; j < x + length; j++)
            for (int k = z - width; k < z + width; k++)
            for (int l = y - height; l < y + height; l++)
                if (chunk.blocks[j, l, k] == 0 && Random.value < 0.6)
                    chunk.blocks[j, l, k] = blockType;
        }
    }
    
    /// <summary>
    /// Generates a mall in the given position
    /// </summary>
    /// <param name="world">World object</param>
    /// <param name="position">Position of the mall</param>
    public static void GenerateMall(World world, Vector3 position, int xOffset, int zOffset)
    {
        int[,,] mall = GenerateCuboid(mallX, mallY, mallZ, mallOffset, 8);
        int[,,] entrance = GenerateStructureFromFile("Assets/Scripts/Structures/MallEntrance.txt");
        mall = MergeArrays(mall, entrance, new Vector3(mallX / 2 + mallOffset, 0, 0));
        int[,,] eiffel = GenerateStructureFromFile("Assets/Scripts/Structures/Eiffel.txt");

        int x = 0, y = -1, z = 0, iterations = 0;
        bool stopsignal = false;
        while (y == -1)
        {
            iterations++;
            if (iterations == 50000)
            {
                stopsignal = true;
                break;
            }
            x = Random.Range((int)position.x - xOffset + mallX, (int)position.x + xOffset - mallX);
            z = Random.Range((int)position.z - zOffset + mallZ, (int)position.z + zOffset) - mallZ;

            y = GetYcoord(world, new Vector3(x, 0, z), mallX + mallOffset, mallZ + mallOffset, world.biome.solidGroundHeight + 15);
        }
        if (stopsignal) return;
        
        int chestcount = 6;
        for (int i = 0; i < chestcount; i++)
        {
            PlaceChest(mall, new Vector3(x, y, z));
        }
        
        PlaceStructure(mall, world, new Vector3(x, y, z));
        PlaceStructure(eiffel, world, new Vector3(x, y + 1, z) + new Vector3(13 - mallOffset, 0, -20 + mallOffset));
    }

    public static void GenerateBuildings(World world, Vector3 position, int xOffset, int zOffset)
    {
        int numBuildings = (int)(World.WorldSize * Chunk.Width * BuildingsDensity);

        for (int i = 0; i < numBuildings; i++)
        {
            int buildingLength = Random.Range(8, 20);
            int buildingHeight = Random.Range(6, 15);
            int buildingWidth = Random.Range(8, 20);
            int[,,] building = GenerateCuboid(buildingLength, buildingHeight, buildingWidth, 1, 6);

            int[,,] entrance;
            int num = Random.Range(0, 4);
            if (num < 2)
                entrance = GenerateStructureFromFile("Assets/Scripts/Structures/BuildingEntrance1.txt");
            else
                entrance = GenerateStructureFromFile("Assets/Scripts/Structures/BuildingEntrance2.txt");
            
            int entranceX = Random.Range(3, buildingLength - 3);
            int entranceZ = Random.Range(3, buildingWidth - 3);

            if (num == 0)
                entranceZ = 0;
            else if (num == 1)
                entranceZ = buildingWidth;
            else if (num == 2)
                entranceX = 0;
            else
                entranceX = buildingLength;
            
            building = MergeArrays(building, entrance, new Vector3(entranceX, 0, entranceZ));
            
            int y = -1, x = 0, z = 0, iterations = 0;
            bool stopsignal = false;
            while (y == -1)
            {
                iterations++;
                if (iterations == 10000)
                {
                    stopsignal = true;
                    break;
                }
                x = Random.Range((int)position.x - xOffset, (int)position.x + xOffset);
                z = Random.Range((int)position.z - zOffset, (int)position.z + zOffset);
                
                y = GetYcoord(world, new Vector3(x, 0, z), buildingLength + 10, buildingWidth + 10, world.biome.solidGroundHeight + 15);
            }
            if (stopsignal) continue;
            
            int chestcount = 2;
            for (int j = 0; j < chestcount; j++)
            {
                PlaceChest(building, new Vector3(x, y, z));
            }
            
            PlaceStructure(building, world, new Vector3(x, y, z));
        }
    }
    
    private static void PlaceChest(int[,,] building, Vector3 position)
    {
        int x, z;
        while (true)
        {
            x = Random.Range(3, building.GetLength(0) - 3);
            z = Random.Range(building.GetLength(2) / 2, building.GetLength(2) - 3);
            if (building[x, 1, z] != 12) break;
        }
        building[x, 1, z] = 12;
        GameObject chest = UnityEngine.Object.Instantiate(GameObject.Find("Chest block"), position + new Vector3(x, 1, z), Quaternion.identity);
    }
    
    public static void GenerateGrass(Chunk chunk)
    {
        int x, y, z;
        int numGrass = (int)(Math.Pow(Chunk.Width, 2) * GrassDensity);
        for (int i = 0; i < numGrass; i++)
        {
            y = -100;
            x = Random.Range(0, Chunk.Width);
            z = Random.Range(0, Chunk.Width);
            for (int j = Chunk.Height - 1; j >= 0; j--)
                if (chunk.blocks[x, j, z] == 1)
                {
                    y = j + 1;
                    break;
                }
                else if (chunk.blocks[x, j, z] > -1)
                {
                    y = -100;
                    break;
                }

            if (y == -100) continue;
            chunk.blocks[x, y, z] = 4;
        }
    }
    
    /// <summary>
    /// Generates trees in a chunk randomly
    /// </summary>
    /// <param name="chunk">Chunk object</param>
    public static void GenerateTrees(Chunk chunk)
    {
        int numTrees = (int)(Math.Pow(Chunk.Width, 2) * TreeDensity);
        int x, y, z;
        for (int i = 0; i < numTrees; i++)
        {
            y = -100;
            x = Random.Range(3, Chunk.Width - 4);
            z = Random.Range(3, Chunk.Width - 4);
            for (int j = Chunk.Height - 1; j >= 0; j--)
                if (chunk.blocks[x, j, z] == 1)
                {
                    y = j + 1;
                    break;
                }
                else if (chunk.blocks[x, j, z] > -1 || chunk.blocks[x - 1, j, z] > -1 ||
                         chunk.blocks[x, j, z - 1] > -1 || chunk.blocks[x + 1, j, z] > -1 || chunk.blocks[x, j, z + 1] > -1)
                {
                    y = -100;
                    break;
                }

            if (y == -100) continue;
            GenerateTree(new Vector3(x, y, z), chunk);
        }
    }

    /// <summary>
    /// Merges arr2 into arr1 at the desired position
    /// </summary>
    /// <param name="arr1">Array 1</param>
    /// <param name="arr2">Array 2</param>
    /// <param name="position">XYZ position</param>
    /// <returns>Merged array</returns>
    private static int[,,] MergeArrays(int[,,] arr1, int[,,] arr2, Vector3 position)
    {
        int indx, indy, indz;
        indx = 0;
        for (int i = (int)position.x; i < position.x + arr2.GetLength(0); i++)
        {
            indy = 0;
            for (int j = (int)position.y; j < position.y + arr2.GetLength(1); j++)
            {
                indz = 0;
                for (int k = (int)position.z; k < position.z + arr2.GetLength(2); k++)
                {
                    arr1[i, j, k] = arr2[indx, indy, indz];
                    indz++;
                }
                indy++;
            }
            indx++;
        }
        return arr1;
    }

    /// <summary>
    /// Generates a structure (in an array) that is defined in a file
    /// </summary>
    /// <param name="fileName">Path to the structure file</param>
    /// <returns>Structure array</returns>
    private static int[,,] GenerateStructureFromFile(string fileName)
    {
        int[,,] arr = new int[1,1,1];
        using (StreamReader read = new StreamReader(fileName))
        {
            string line;
            string[] parts;
            while ((line = read.ReadLine()) != null)
            {
                if (line == "size:")
                {
                    //getting size of the array
                    line = read.ReadLine();
                    if (line == null) continue;
                    parts = line.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    arr = new int[int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2])];
                    
                    //filling array with air blocks
                    for (int i = 0; i < arr.GetLength(0); i++)
                        for (int j = 0; j < arr.GetLength(1); j++)
                            for (int k = 0; k < arr.GetLength(2); k++)
                                arr[i, j, k] = -1;
                }
                else if (line == "blocks:")
                {
                    string blockline;
                    int blockType = -1;
                    while ((blockline = read.ReadLine()) != null)
                    {
                        if (blockline[0] == '.')
                        {
                            //getting block type
                            blockType = int.Parse(blockline.Substring(1, blockline.Length - 1));
                        }
                        else
                        {
                            //getting block positions
                            parts = blockline.Split(',', StringSplitOptions.RemoveEmptyEntries);
                            arr[int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2])] = blockType;
                        }
                    }
                    break;
                }
                
            }
        }

        return arr;
    }
    
    /// <summary>
    /// Generates a cuboid of the given dimensions
    /// </summary>
    /// <param name="x">X length of the structure</param>
    /// <param name="y">Y height of the structure</param>
    /// <param name="z">Z length of the structure</param>
    /// <param name="offset">Offset of the structure in the array</param>
    /// <returns>Array of the generated cuboid</returns>
    private static int[,,] GenerateCuboid(int x, int y, int z, int offset, int blockType)
    {
        int[,,] arr = new int[x + 2 * offset, y + offset, z + 2 * offset];
        
        for (int i = 0;  i < x + 2 * offset; i++)
        for (int j = 0; j  < y + offset; j++)
        for (int k = 0; k < z + 2 * offset; k++)
            if ((i == offset || i == x + offset - 1) && k >= offset && k < z + offset || 
                (k == offset || k == z + offset - 1) && i >= offset && i < x + offset || 
                (j == 0 || j == y - 1 + offset) && k >= offset && k < z + offset - 1 && i >= offset && i < x + offset - 1)
                arr[i, j, k] = blockType;
            else
                arr[i, j, k] = -1;
        
        for (int i = 0; i < x + 2 * offset; i++)
            for (int k = 0; k < z + 2 * offset; k++)
                if (arr[i, 0, k] == -1)
                    arr[i, 0, k] = 1;
        return arr;
    }
    
    /// <summary>
    /// Places given structure in the given position in the world
    /// </summary>
    /// <param name="arr">Array of the structure</param>
    /// <param name="world">World object</param>
    /// <param name="position">Position of the structure</param>
    private static void PlaceStructure(int[,,] arr, World world, Vector3 position)
    {
        for (int i = (int)position.x; i < position.x + arr.GetLength(0); i++)
        {
            for (int j = (int)position.z; j < (int)position.z + arr.GetLength(2); j++)
            {
                int currentChunkX = i / Chunk.Width;
                int currentChunkZ = j / Chunk.Width;
                int chunkX = i % Chunk.Width;
                int chunkZ = j % Chunk.Width;
                for (int o = (int)position.y; o < (int)position.y + arr.GetLength(1); o++)
                {
                    world.chunks[currentChunkX, currentChunkZ].blocks[chunkX, o, chunkZ] = arr[i - (int)position.x, o - (int)position.y, j - (int)position.z];
                }
            }
        }
    }

    /// <summary>
    /// Gets the Y coordinate of the place where the entire perimeter of the structure touches the ground
    /// </summary>
    /// <param name="world">World object</param>
    /// <param name="position">position of the structure</param>
    /// <param name="X">X length of the structure</param>
    /// <param name="Z">Z length of the structure</param>
    /// <returns>calculated Y coordinate</returns>
    private static int GetYcoord(World world, Vector3 position, int X, int Z, int from = 224)
    {
        int currentChunkX, currentChunkZ, chunkX, chunkZ;
        bool dirtfound, airfound, tolerance = false;

        for (int y = from; y >= world.biome.solidGroundHeight + 6; y--)
        {
            dirtfound = false;
            airfound = false;
            for (int i = (int)position.x; i < (int)position.x + X; i++)
            {
                for (int j = (int)position.z; j < (int)position.z + Z; j++)
                {
                    currentChunkX = i / Chunk.Width;
                    currentChunkZ = j / Chunk.Width;
                    chunkX = i % Chunk.Width;
                    chunkZ = j % Chunk.Width;
                    if (world.chunks[currentChunkX, currentChunkZ].blocks[chunkX, y, chunkZ] != 1 &&
                        world.chunks[currentChunkX, currentChunkZ].blocks[chunkX, y, chunkZ] != -1) return -1;
                    
                    if (world.chunks[currentChunkX, currentChunkZ].blocks[chunkX, y, chunkZ] == -1) airfound = true;
                    if (world.chunks[currentChunkX, currentChunkZ].blocks[chunkX, y, chunkZ] == 1) dirtfound = true;

                    if (airfound && dirtfound)
                    {
                        if (tolerance) return -1;
                        else
                        {
                            tolerance = true;
                            break;
                        }
                    }
                }
                if (dirtfound && airfound) break;
            }
            if (dirtfound && !airfound) return y;
        }

        return -1;
    }
    
    /// <summary>
    /// Generates one tree within a specified chunk
    /// </summary>
    /// <param name="position">Tree position in a chunk</param>
    /// <param name="chunk">Chunk object</param>
    private static void GenerateTree(Vector3 position, Chunk chunk)
    {
        int trunkHeight = Random.Range(3, 8);
        int leafWidth = Random.Range(5, 7);
        int leafHeight = Random.Range(trunkHeight + 5, trunkHeight + 9);
        int maxLeavesOnTop = 1;
        
        //Generating trunk
        for (int i = 0; i <= trunkHeight; i++)
            chunk.blocks[(int)position.x, i + (int)position.y, (int)position.z] = 2;
        
        //Generating leaves
        for (int j = trunkHeight; j <= leafHeight; j++)
        {   
            if (maxLeavesOnTop == 0) break;
            if ((j - trunkHeight + 1) % 2 == 0 && leafWidth != 1) leafWidth -= 2;
            if (leafWidth == 1) maxLeavesOnTop--;
            for (int i = -leafWidth / 2; i <= leafWidth / 2; i++)
                for (int k = -leafWidth / 2; k <= leafWidth / 2; k++)
                {
                    if (i == 0 && j == trunkHeight && k == 0) continue;
                    if (leafWidth != 1 && Math.Abs(i) == leafWidth / 2 && Math.Abs(k) == leafWidth / 2) continue;
                    if (chunk.blocks[i + (int)position.x, j + (int)position.y, k + (int)position.z] != -1) continue; 
                    chunk.blocks[i + (int)position.x, j + (int)position.y, k + (int)position.z] = 3;
                }
        }
    }
}