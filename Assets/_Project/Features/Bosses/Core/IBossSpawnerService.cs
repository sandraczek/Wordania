using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Core.Data;
using Wordania.Core.Identifiers;
using Wordania.Core.Services;
using Wordania.Features.Bosses.Data;

namespace Wordania.Features.Bosses.Core
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