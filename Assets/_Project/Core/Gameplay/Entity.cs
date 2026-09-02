using UnityEngine;
using Wordania.Core.Identifiers;

namespace Wordania.Core.Gameplay
{
    public class Entity : MonoBehaviour, IEntityContext
    {
        public InstanceId InstanceId { get; private set; }
        public Transform Transform => transform;

        public bool TryGetFeature<T>(out T feature) where T : class
        {
            feature = GetComponentInChildren<T>();
            return feature != null;
        }

        private void Start()
        {
            InstanceId = GetComponent<IEntity>().InstanceId;
        }
    }
}