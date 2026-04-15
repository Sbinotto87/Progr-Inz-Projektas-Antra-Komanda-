using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.LightTransport;

namespace Assets.Scripts
{
    public static class PerlinNoise
    {
        public static bool Get3DPerlinNoise(UnityEngine.Vector3 position, float offset, float scale, float threshold)
        {
            float x = (position.x + offset + 0.1f) * scale;
            float y = (position.y + offset + 0.1f) * scale;
            float z = (position.z + offset + 0.1f) * scale;

            float AB = Mathf.PerlinNoise(x, y);
            float BC = Mathf.PerlinNoise(y, z);
            float AC = Mathf.PerlinNoise(x, z);
            float BA = Mathf.PerlinNoise(y, x);
            float CB = Mathf.PerlinNoise(z, y);
            float CA = Mathf.PerlinNoise(z, x);

            if ((AB + BC + AC + BA + CB + CA) / 6f > threshold)
                return true;
            else
                return false;
        }
        public static float Get2DPerlinNoise(UnityEngine.Vector2 position , float offset, float scale, float offsetX, float offsetZ)
        {
            float2 noiseInput = new float2(position.x + 0.1f + offsetX, position.y + 0.1f + offsetZ) * (scale / Chunk.Width);
            float noiseValue = (noise.snoise(noiseInput) + 1f) * 0.5f; //normalize noise for 0 - 1
            return noiseValue;
        }
    }
}
