using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Data;
using Wordania.Identifiers;

namespace Wordania.Bosses.Core
{
    public interface IBossSpawnerService
    {
        /// <summary>
        /// Host: attempts to spawn a boss at the given position. 
        /// Returns the instance if successful, or null if it fails.
        /// </summary>
        BossController SpawnBoss(AssetId bossId, Vector2 position);

        /// <summary>Client: recreates a boss the host spawned, with the host's id. No gameplay events.</summary>
        BossController SpawnReplica(AssetId bossId, Vector2 position, InstanceId instanceId);
    }
}