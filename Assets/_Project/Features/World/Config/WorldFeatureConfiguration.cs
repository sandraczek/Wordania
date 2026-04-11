using UnityEngine;
using System.Collections.Generic;

namespace Wordania.Features.World.Config
{
    [CreateAssetMenu(fileName = "WorldFeaturesConfig", menuName = "World/Features Config")]
    public class WorldFeatureConfiguration : ScriptableObject
    {
        [field: Header("Ores & Minerals")]
        [field: SerializeField] public List<FeatureSpawnRule> Ores { get; private set; } = new();

        [field: Header("Stone Variations")]
        [field: SerializeField] public List<FeatureSpawnRule> StoneVariations { get; private set; } = new();
    }
}