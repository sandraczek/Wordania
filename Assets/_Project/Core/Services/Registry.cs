using System.Collections.Generic;
using Wordania.Core.Identifiers;

namespace Wordania.Core.Services
{
    public abstract class Registry<T> : IRegistry<T> where T : IEntity
    {
        private readonly List<T> _entities = new();
        private readonly Dictionary<InstanceId, int> _idToIndexMap = new();

        public int Count => _entities.Count;
        public IReadOnlyList<T> GetAll() => _entities;

        public virtual void Register(T entity)
        {
            if (!_idToIndexMap.ContainsKey(entity.InstanceId))
            {
                int newIndex = _entities.Count;
                _entities.Add(entity);
                _idToIndexMap.Add(entity.InstanceId, newIndex);
            }
        }

        public virtual void Unregister(InstanceId entityId)
        {
            if (_idToIndexMap.TryGetValue(entityId, out int indexToRemove))
            {
                int lastIndex = _entities.Count - 1;
                T lastEntity = _entities[lastIndex];

                _entities[indexToRemove] = lastEntity;

                _idToIndexMap[lastEntity.InstanceId] = indexToRemove;

                _entities.RemoveAt(lastIndex);
                _idToIndexMap.Remove(entityId);
            }
        }

        public bool TryGet(InstanceId entityId, out T entity)
        {
            if (_idToIndexMap.TryGetValue(entityId, out int index))
            {
                entity = _entities[index];
                return true;
            }

            entity = default;
            return false;
        }
    }
}