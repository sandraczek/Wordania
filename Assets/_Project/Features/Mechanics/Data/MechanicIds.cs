using System.Linq;
using UnityEngine;
using Wordania.Core.Data;
using Wordania.Core.Identifiers;
using Wordania.Features.Mechanics.Data;

namespace Wordania.Features.Mechanics.Data
{
    public class MechanicIds
    {

        public readonly AssetId Mining;

        public MechanicIds(IAssetRegistry<MechanicData> registry)
        {
            Mining = GetMechanicId<MiningMechanicData>(registry);
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