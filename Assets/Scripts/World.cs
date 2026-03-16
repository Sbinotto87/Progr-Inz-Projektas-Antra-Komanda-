using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    
    public class World: MonoBehaviour
    {
        /// <summary>
        /// Player gameObject prefab
        /// </summary>
        [SerializeField]
        private GameObject Player;
        /// <summary>
        /// UiCanvas
        /// </summary>
        [SerializeField]
        private Canvas UICanvas;
        /// <summary>
        /// world size
        /// </summary>
        public const int WorldSize = 8;
        /// <summary>
        /// chunk array
        /// </summary>
        public Chunk[,] chunks = new Chunk[WorldSize, WorldSize];
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
        public int Tick;

        private void Awake()
        {
            var player = Instantiate(Player, new Vector3(20, 20, 20), Quaternion.identity); //for testing   Create player prefab at 20, 20, 20 coords
            player.name = Player.name;
            var canvas = Instantiate(UICanvas);
            canvas.name = UICanvas.name;
        }
        private void Start()
        {
            DayTime = 0;
            CurrentDay = 0;
            Tick = 0;

            GenerateWorld();



        }
        private void Update()
        {

        }
        /// <summary>
        /// tickrate, 20 ticks per second, used for ingame time
        /// </summary>
        private void FixedUpdate()
        {
            TickDayTime();
        }
        /// <summary>
        /// updates the ingame time (in seconds) every 20 tics
        /// </summary>
        private void TickDayTime()
        {
            switch (Tick)
            {
                case 19:
                    Tick = 0;
                    DayTime++;
                    if (DayTime == 1440 - 1) UpdateDayCounter();
                    break;
                default:
                    Tick++;
                    break;
            }
        }
        /// <summary>
        /// increments day counter and resets day time, called at midnight
        /// </summary>
        private void UpdateDayCounter()
        {
            DayTime = 0;
            CurrentDay++;
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
