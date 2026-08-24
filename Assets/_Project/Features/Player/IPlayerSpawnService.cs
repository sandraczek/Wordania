using UnityEngine;
using Wordania.Core.Identifiers;

namespace Wordania.Features.Player
{
    public interface IPlayerSpawnService
    {
        public void SetSpawn(InstanceId id, Vector2 spawn);
        public Vector2 GetSpawn(InstanceId id);
        public Vector2 GetWorldSpawn();
        public void ClearSpawn(InstanceId id);
    }
}