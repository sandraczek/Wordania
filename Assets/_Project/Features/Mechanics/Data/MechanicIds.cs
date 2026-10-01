using System.Linq;
using UnityEngine;
using Wordania.Data;
using Wordania.Identifiers;

namespace Wordania.Mechanics.Data
{
    public class MechanicIds
    {
        public readonly AssetId Mining;
        public readonly AssetId Building;
        public readonly AssetId GodMode;
        public MechanicIds(IAssetRegistry<MechanicData> registry)
        {
            Mining = GetMechanicId<MiningMechanicData>(registry);
            Building = GetMechanicId<BuildingMechanicData>(registry);
            GodMode = GetMechanicId<GodModeMechanicData>(registry);
        }

        private AssetId GetMechanicId<T>(IAssetRegistry<MechanicData> registry) where T : MechanicData
        {
            var v = registry.Assets.OfType<T>().FirstOrDefault();
            if (v == null)
            {
                Debug.LogWarning($"[MechanicIds] No mechanic of type {typeof(T).Name} found in registry.");
            }
            return v.Id;
        }

    }
}