using System.Collections.Generic;
using Wordania.Core.Gameplay;
using Wordania.Core.Identifiers;
using Wordania.Features.Identifiers;
using Wordania.Features.Player;

namespace Wordania.Core.Services
{
    public interface IEntityRegistry
    {
        IReadOnlyDictionary<InstanceId, Entity> Entities { get; }
        IReadOnlyList<Entity> Players { get; }
        IReadOnlyCollection<Entity> Enemies { get; }
        IReadOnlyCollection<ITrackable> Trackables { get; }

        void Register(Entity entity, InstanceId instanceId);
        void Unregister(InstanceId id);

        bool TryGetPersistentId(InstanceId instanceId, out PersistentId persistentId);
        InstanceId GetInstanceId(PersistentId persistentId);
        bool IsPlayer(InstanceId instanceId);
    }

    public class EntityRegistry : IEntityRegistry
    {
        private readonly Dictionary<InstanceId, Entity> _entities = new();
        private readonly List<Entity> _players = new();
        private readonly HashSet<Entity> _enemies = new();
        private readonly HashSet<ITrackable> _trackables = new();

        private readonly Dictionary<InstanceId, PersistentId> _persistentMap = new();
        private readonly Dictionary<PersistentId, InstanceId> _instanceMap = new();

        public IReadOnlyDictionary<InstanceId, Entity> Entities => _entities;
        public IReadOnlyList<Entity> Players => _players;
        public IReadOnlyCollection<Entity> Enemies => _enemies;
        public IReadOnlyCollection<ITrackable> Trackables => _trackables;

        public void Register(Entity entity, InstanceId instanceId)
        {
            _entities[instanceId] = entity;

            if (entity.TryGetFeature<IPersistent>(out var persistentEntity))
            {
                _persistentMap[instanceId] = persistentEntity.PersistentId;
                _instanceMap[persistentEntity.PersistentId] = instanceId;
            }

            if (entity.TryGetFeature<Player>(out _))
            {
                _players.Add(entity);
            }
            if (entity.TryGetFeature<IEnemy>(out _))
            {
                _enemies.Add(entity);
            }
            if (entity.TryGetFeature<ITrackable>(out var trackable))
            {
                _trackables.Add(trackable);
            }
        }

        public void Unregister(InstanceId id)
        {
            if (_entities.TryGetValue(id, out var entity))
            {
                if (_persistentMap.TryGetValue(id, out var persistentId))
                    _instanceMap.Remove(persistentId);
                _entities.Remove(id);
                _players.Remove(entity);
                _enemies.Remove(entity);
                if (entity.TryGetFeature(out ITrackable t))
                    _trackables.Remove(t);
                _persistentMap.Remove(id);
            }
        }

        public bool TryGetPersistentId(InstanceId instanceId, out PersistentId persistentId)
        {
            return _persistentMap.TryGetValue(instanceId, out persistentId);
        }
        public InstanceId GetInstanceId(PersistentId persistentId)
        {
            return _instanceMap[persistentId];
        }

        public bool IsPlayer(InstanceId instanceId)
        {
            if (_entities.TryGetValue(instanceId, out var entity))
            {
                return entity.TryGetFeature<Player>(out _);
            }
            return false;
        }
    }
}