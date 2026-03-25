using System;
using Assets.Scripts;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Structures
{
    static readonly double TreeDensity = 0.5;
    static readonly double GrassDensity = 1.5;
    
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
                    if (chunk.blocks[i + (int)position.x, j + (int)position.y, k + (int)position.z] == 2) continue; 
                    chunk.blocks[i + (int)position.x, j + (int)position.y, k + (int)position.z] = 3;
                }
        }
    }

    public static void GenerateGrass(World world)
    {
        int randX, randZ, x, y, z, chunkX, chunkZ;
        int numGrass = (int)(world.WorldSizeInBlocks * GrassDensity);
        for (int i = 0; i <= numGrass; i++)
        {
            y = -100;
            randX = Random.Range((World.WorldSize / 2 - world.viewDistance) * Chunk.Width, (World.WorldSize / 2 + world.viewDistance) * Chunk.Width);
            randZ = Random.Range((World.WorldSize / 2 - world.viewDistance) * Chunk.Width, (World.WorldSize / 2 + world.viewDistance) * Chunk.Width);
            x = randX / Chunk.Width;
            z = randZ / Chunk.Width;
            chunkX = randX % Chunk.Width;
            chunkZ = randZ % Chunk.Width;
            for (int j = Chunk.Height - 1; j >= 0; j--)
                if (world.chunks[x, z].blocks[chunkX, j, chunkZ] == 1)
                {
                    y = j + 1;
                    break;
                }
                else if (world.chunks[x, z].blocks[chunkX, j, chunkZ] > -1)
                {
                    y = -100;
                    break;
                }
            if (y == -100)
            {
                i--;
                continue;
            }
            world.chunks[x, z].blocks[chunkX, y, chunkZ] = 4;
        }
    }
    
    /// <summary>
    /// Generates trees in a world randomly
    /// </summary>
    /// <param name="world">World object</param>
    public static void GenerateTrees(World world)
    {
        int numTrees = (int)(world.WorldSizeInBlocks * TreeDensity);
        int randX, randZ, x, y, z, chunkX, chunkZ;
        for (int i = 0; i < numTrees; i++)
        {
            y = -100;
            randX = Random.Range((World.WorldSize / 2 - world.viewDistance) * Chunk.Width, (World.WorldSize / 2 + world.viewDistance) * Chunk.Width);
            randZ = Random.Range((World.WorldSize / 2 - world.viewDistance) * Chunk.Width, (World.WorldSize / 2 + world.viewDistance) * Chunk.Width);
            x = randX / Chunk.Width;
            z = randZ / Chunk.Width;
            chunkX = randX % Chunk.Width;
            chunkZ = randZ % Chunk.Width;
            if (chunkX - 3 < 0 || chunkX + 3 > Chunk.Width - 1 || chunkZ - 3 < 0 || chunkZ + 3 > Chunk.Width - 1)
            {
                i--;
                continue;
            }
            for (int j = Chunk.Height - 1; j >= 0; j--)
                if (world.chunks[x, z].blocks[chunkX, j, chunkZ] == 1)
                {
                    y = j + 1;
                    break;
                }
                else if (world.chunks[x, z].blocks[chunkX, j, chunkZ] > -1)
                {
                    y = -100;
                    break;
                }
            if (y == -100)
            {
                i--;
                continue;
            }
            GenerateTree(new Vector3(chunkX, y, chunkZ), world.chunks[x, z]);
        }
    }
}