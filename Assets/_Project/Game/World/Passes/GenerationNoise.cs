using UnityEngine;

namespace Wordania.World.Passes
{
    /// <summary>Deterministic noise helpers shared by the generation passes.</summary>
    public static class GenerationNoise
    {
        /// <summary>
        /// Fractal 1D Perlin noise normalized to 0..1. <paramref name="t"/> is the seeded coordinate
        /// (e.g. x + seed for the horizon, y + seed for the great wall).
        /// </summary>
        public static float Fbm1D(float t, float frequency, int octaves, float persistence, float lacunarity)
        {
            float total = 0;
            float amplitude = 1f;
            float maxValue = 0;

            for (int i = 0; i < octaves; i++)
            {
                float sample = t * frequency + (i * 1000);
                total += Mathf.PerlinNoise(sample, 0) * amplitude;
                maxValue += amplitude;

                amplitude *= persistence;
                frequency *= lacunarity;
            }

            return total / maxValue;
        }

        /// <summary>Integer hash of two coordinates and a seed (avalanche mix).</summary>
        public static uint Hash(int a, int b, int seed)
        {
            unchecked
            {
                uint h = (uint)seed * 0x9E3779B1u;
                h ^= (uint)a * 0x85EBCA77u;
                h = (h << 13) | (h >> 19);
                h *= 0xC2B2AE3Du;
                h ^= (uint)b * 0x27D4EB2Fu;
                h = (h << 17) | (h >> 15);
                h *= 0x165667B1u;
                h ^= h >> 15;
                h *= 0x85EBCA77u;
                h ^= h >> 13;
                h *= 0xC2B2AE3Du;
                h ^= h >> 16;
                return h;
            }
        }

        /// <summary>Hash mapped to [0, 1).</summary>
        public static float Hash01(int a, int b, int seed) => (Hash(a, b, seed) >> 8) * (1f / 16777216f);
    }
}
