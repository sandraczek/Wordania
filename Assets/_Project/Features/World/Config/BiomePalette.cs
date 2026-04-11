using UnityEngine;
using Wordania.Core.Data; // Assuming BlockData is accessible from here or a specific block namespace

namespace Wordania.Features.World.Config
{
    /// <summary>
    /// Defines the visual and physical composition of a specific biome.
    /// </summary>
    [CreateAssetMenu(fileName = "Palette_NewBiome", menuName = "World/Biome Palette")]
    public class BiomePalette : ScriptableObject
    {
        [field: Header("Terrain Layers")]

        [field: Tooltip("The top-most layer block exposed to the air (e.g., Grass, Sand, Corrupted Grass).")]
        [field: SerializeField] public BlockData SurfaceBlock { get; private set; }
        [field: SerializeField] public BlockData SurfaceWall { get; private set; }

        [field: Tooltip("The layer directly beneath the surface (e.g., Dirt, Sand, Corrupted Dirt).")]
        [field: SerializeField] public BlockData SubSurfaceBlock { get; private set; }
        [field: SerializeField] public BlockData SubSurfaceWall { get; private set; }

        [field: Tooltip("The primary filler block for deep underground (e.g., Stone, Hardened Sand, Ebonstone).")]
        [field: SerializeField] public BlockData UndergroundBlock { get; private set; }
        [field: SerializeField] public BlockData UndergroundWall { get; private set; }

        [field: Header("Structural Boundaries")]

        [field: Tooltip("Indestructible block used to frame the world boundaries (End of the map).")]
        [field: SerializeField] public BlockData BarrierBlock { get; private set; }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (SurfaceBlock == null || SubSurfaceBlock == null || UndergroundBlock == null || SurfaceWall == null || SubSurfaceWall == null || UndergroundWall == null)
            {
                Debug.LogWarning($"[{nameof(BiomePalette)}] The palette '{name}' is missing essential terrain blocks. World generation will be incomplete.");
            }
        }
#endif
    }
}