using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Data;
using Wordania.Identifiers;
using Wordania.Bosses.Data;

namespace Wordania.Bosses.Core
{
    public interface IBossSpawnerService
    {
        /// <summary>
        /// Attempts to spawn a boss at the given position. 
        /// Returns the instance if successful, or null if it fails.
        /// </summary>
        BossController SpawnBoss(AssetId bossId, Vector2 position);
    }
}