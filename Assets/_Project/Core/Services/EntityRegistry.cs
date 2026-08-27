using System.Collections.Generic;
using Wordania.Core.Combat;
using Wordania.Core.Gameplay;
using Wordania.Core.Identifiers;
using Wordania.Features.Player;

namespace Wordania.Core.Services
{
    public interface IEntityRegistry
    {
        IReadOnlyList<Entity> ActivePlayers { get; }
        void Register(Entity context);
        void Unregister(InstanceId id);
    }
    public class EntityRegistry
    {
        private readonly Dictionary<InstanceId, Entity> _entities = new();

        public IReadOnlyList<Entity> ActivePlayers => _players;
        private readonly List<Entity> _players = new();

        public void Register(Entity context)
        {
            _entities[context.InstanceId] = context;

            if (context.TryGetFeature<Player>(out _))
            {
                _players.Add(context);
            }
        }

        public void Unregister(InstanceId id)
        {
            if (_entities.TryGetValue(id, out var context))
            {
                _entities.Remove(id);
                _players.Remove(context);
            }
        }
    }
}