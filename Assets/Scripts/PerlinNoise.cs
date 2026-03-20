using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public static class PerlinNoise
    {
        public static float Get2DPerlinNoise(UnityEngine.Vector2 position , float offset, float scale)
        {
            return Mathf.PerlinNoise((position.x + 0.1f) / Chunk.Width * scale + offset, (position.y + 0.1f) / Chunk.Width * scale + offset);
        }
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
    }
}
