using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.Gameplay;
using Wordania.Enemies.Data;
using Wordania.Identifiers;
using Wordania.Enemies.AI;

namespace Wordania.Enemies.Core
{
    public interface IEnemyFactory
    {
        /// <summary>Host: spawns a new enemy and announces it to clients. Context = world knowledge for its AI.</summary>
        IEnemy CreateEnemy(EnemyTemplate data, Vector3 position, EnemySpawnContext context = default);
        /// <summary>Client: recreates an enemy the host spawned, with the host's id.</summary>
        IEnemy CreateReplica(EnemyTemplate data, Vector3 position, InstanceId instanceId);
        public UniTask PrewarmPoolAsync(EnemyTemplate template);
    }
}