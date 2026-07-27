using UnityEngine;
using Wordania.Core.Identifiers;

namespace Wordania.Core.Gameplay
{
    public interface IEntityContext
    {
        Transform Transform { get; }
        bool TryGetFeature<T>(out T feature) where T : class;
    }
}