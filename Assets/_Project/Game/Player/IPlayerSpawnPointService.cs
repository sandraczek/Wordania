using UnityEngine;
using Wordania.Identifiers;

namespace Wordania.Player
{
    public interface IPlayerSpawnPointService
    {
        public void SetSpawn(InstanceId id, Vector2 spawn);
        public Vector2 GetSpawn(InstanceId id);
        public Vector2 GetWorldSpawn();
        public void ClearSpawn(InstanceId id);
    }
}