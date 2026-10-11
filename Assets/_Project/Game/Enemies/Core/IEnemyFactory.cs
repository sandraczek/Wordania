using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.Gameplay;
using Wordania.Enemies.Data;
using Wordania.Identifiers;

namespace Wordania.Enemies.Core
{
    public interface IEnemyFactory
    {
        /// <summary>Host: spawns a new enemy and announces it to clients.</summary>
        IEnemy CreateEnemy(EnemyTemplate data, Vector3 position);
        /// <summary>Client: recreates an enemy the host spawned, with the host's id.</summary>
        IEnemy CreateReplica(EnemyTemplate data, Vector3 position, InstanceId instanceId);
        public UniTask PrewarmPoolAsync(EnemyTemplate template);
    }
}