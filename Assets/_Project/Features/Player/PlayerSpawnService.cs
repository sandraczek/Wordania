using System.Collections.Generic;
using UnityEngine;
using Wordania.Core.Identifiers;
using Wordania.Features.World;

namespace Wordania.Features.Player
{
    public class PlayerSpawnService : IPlayerSpawnService
    {
        private readonly IWorldService _world;

        private readonly Dictionary<InstanceId, Vector2> _spawns = new();

        public PlayerSpawnService(IWorldService world)
        {
            _world = world;
        }
        public void SetSpawn(InstanceId id, Vector2 spawn)
        {
            _spawns[id] = spawn;
        }
        public Vector2 GetSpawn(InstanceId id)
        {
            if (_spawns.TryGetValue(id, out Vector2 spawn))
            {
                return spawn;
            }

            return _world.GetSpawnPoint();
        }
        public Vector2 GetWorldSpawn()
        {
            return _world.GetSpawnPoint();
        }
        public void ClearSpawn(InstanceId id)
        {
            _spawns.Remove(id);
        }
    }
}