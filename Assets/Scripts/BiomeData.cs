using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    [CreateAssetMenu(fileName = "BiomeAttributes", menuName = "NaftosMiskas/Biomes")]
    public class BiomeData : ScriptableObject
    {
        public string BiomeName;

        public int solidGroundHeight;
        public int terrainHeight;
        public float terrainScale;

        public Lode[] lodes;
    }
    public class Lode
    {
        public string lodeName;
        public byte blockID;
        public int minHeight;
        public int maxHeight;
        public float scale;
        public float threshold;
        public float noiseOffset;
    }
}
