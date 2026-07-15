using UnityEngine;

namespace Wordania.Core.Gameplay
{
    public class EntityContext : MonoBehaviour, IEntityContext
    {
        public Transform Transform => transform;

        public bool TryGetFeature<T>(out T feature) where T : class
        {
            feature = GetComponentInChildren<T>();
            return feature != null;
        }
    }
}