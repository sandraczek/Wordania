using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wordania.Structures
{
    /// <summary>Which structures a generated world contains and where they may appear.</summary>
    [CreateAssetMenu(fileName = "WorldStructureConfiguration", menuName = "World/Structure Configuration")]
    public sealed class WorldStructureConfiguration : ScriptableObject
    {
        public enum Placement
        {
            /// <summary>Bottom row replaces the surface (median of the covered columns).</summary>
            Surface,
            /// <summary>Buried below the surface; '.' cells carve the rooms out of the rock.</summary>
            Underground,
        }

        [Serializable]
        public struct SpawnRule
        {
            public StructureTemplate Template;
            [Min(0)] public int Count;
            public Placement Placement;
            [Tooltip("Minimum horizontal distance (tiles) from the world spawn.")]
            [Min(0)] public int MinDistanceFromSpawn;

            [Header("Surface")]
            [Tooltip("Max height difference of the surface under the structure (tiles). Lower = only flat places.")]
            [Min(0)] public int MaxSurfaceVariation;

            [Header("Underground")]
            [Tooltip("Tiles between the surface and the structure's top row (measured at its center column).")]
            [Min(0)] public int MinDepth;
            [Min(0)] public int MaxDepth;
        }

        [Tooltip("Placed in the middle of every new world. Its player spawn marker becomes the world spawn.")]
        public StructureTemplate StartingStructure;
        [Tooltip("Free tiles kept between structures (in every direction).")]
        [Min(0)] public int Spacing = 8;
        public List<SpawnRule> Rules = new();
    }
}
