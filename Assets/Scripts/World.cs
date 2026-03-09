using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    
    public class World: MonoBehaviour
    {
        /// <summary>
        /// world size
        /// </summary>
        static int WorldSize = 8;
        /// <summary>
        /// chunk array
        /// </summary>
        Chunk[,] chunks = new Chunk[WorldSize, WorldSize];
        /// <summary>
        /// no clu, pavogiau koda
        /// </summary>
        List<ChunkCoord> activeChunks = new List<ChunkCoord>();
        /// <summary>
        /// no clu, pavogiau koda
        /// </summary>
        public Material material;
        /// <summary>
        /// world time
        /// </summary>
        public int DayTime; //1440 seconds (24 minutes, 1 irl second = 1 ingame minute
        public int CurrentDay; //event every 7 days?

        private void Start()
        {
            GenerateWorld();
        }

        private void Update()
        {

        }
        /// <summary>
        /// generates chunk coordinates for world and the chunks themselves
        /// </summary>
        private void GenerateWorld()
        {
            for (int x = 0; x < WorldSize; x++)
            {
                for (int z = 0; z < WorldSize; z++)
                {
                    CreateChunk(new ChunkCoord(x, z));
                }
            }
        }
        /// <summary>
        /// creates chunk game object
        /// </summary>
        /// <param name="coord">coordiate to create chunk at</param>
        private void CreateChunk(ChunkCoord coord)
        {
            chunks[coord.x, coord.z] = new Chunk(new ChunkCoord(coord.x, coord.z), this);
            activeChunks.Add(new ChunkCoord(coord.x, coord.z));
        }
    }
    /// <summary>
    /// chunk coordinate class
    /// </summary>
    public class ChunkCoord
    {
        public int x;
        public int z;
        public ChunkCoord(int _x, int _z)
        {
            x = _x;
            z = _z;
        }
    }
}
