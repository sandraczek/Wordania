using System.Collections.Generic;
using Wordania.Core.Gameplay;
using Wordania.Core.Identifiers;
using Wordania.Features.Identifiers;
using Wordania.Features.Player;

namespace Wordania.Core.Services
{
    public interface IEntityRegistry
    {
        IReadOnlyDictionary<InstanceId, IEntityContext> Entities { get; }
        IReadOnlyList<IEntityContext> ActivePlayers { get; }

        void Register(IEntityContext context, InstanceId instanceId);
        void Unregister(InstanceId id);

        bool TryGetPersistentId(InstanceId instanceId, out PersistentId persistentId);
        InstanceId GetInstanceId(PersistentId persistentId);
        bool IsPlayer(InstanceId instanceId);
    }

    public class EntityRegistry : IEntityRegistry
    {
        private readonly Dictionary<InstanceId, IEntityContext> _entities = new();
        private readonly List<IEntityContext> _players = new();

        private readonly Dictionary<InstanceId, PersistentId> _persistentMap = new();
        private readonly Dictionary<PersistentId, InstanceId> _instanceMap = new();

        public IReadOnlyDictionary<InstanceId, IEntityContext> Entities => _entities;
        public IReadOnlyList<IEntityContext> ActivePlayers => _players;

        public void Register(IEntityContext context, InstanceId instanceId)
        {
            _entities[instanceId] = context;

            if (context.TryGetFeature<IPersistent>(out var persistentEntity))
            {
                _persistentMap[instanceId] = persistentEntity.PersistentId;
                _instanceMap[persistentEntity.PersistentId] = instanceId;
            }

            if (context.TryGetFeature<Player>(out _))
            {
                _players.Add(context);
            }
        }

        public void Unregister(InstanceId id)
        {
            if (_entities.TryGetValue(id, out var context))
            {
                if (_persistentMap.TryGetValue(id, out var persistentId))
                    _instanceMap.Remove(persistentId);
                _entities.Remove(id);
                _players.Remove(context);
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
            if (_entities.TryGetValue(instanceId, out var context))
            {
                return context.TryGetFeature<Player>(out _);
            }
            return false;
        }
    }
}