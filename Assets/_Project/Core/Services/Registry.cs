using System.Collections.Generic;
using Wordania.Core.Identifiers;

namespace Wordania.Core.Services
{
    public abstract class Registry<T>: IRegistry<T> where T:IEntity 
    {
        protected readonly Dictionary<int, T> _registry = new();
        public void Register(T entity)
        {
            _registry[entity.InstanceId] = entity;
        }

        public void Unregister(int entityId)
        {
            _registry.Remove(entityId);
        }
    }
}