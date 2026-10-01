
using System.Collections.Generic;
using Wordania.Identifiers;

namespace Wordania.Data
{
    public interface IAssetRegistry<T> where T : DataAsset
    {
        /// <summary>
        /// Returns null if id is not in registry.
        /// </summary>
        T Get(AssetId id);

        /// <summary>
        /// This list provides all items in the registry. Use only when necesarry.
        /// </summary>
        IReadOnlyList<T> Assets { get; }
    }
}