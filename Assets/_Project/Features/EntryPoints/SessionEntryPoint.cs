using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Features.World;
using Wordania.Features.Player;
using Wordania.Core.Gameplay;
using Wordania.Core;
using Wordania.Features.Services;
using Wordania.Features.HUD;
using Wordania.Core.SaveSystem;
using Wordania.Features.HUD.Loading;
using Wordania.Features.HUD.Saving;
using Wordania.Features.Enemies.Data;
using Wordania.Features.Enemies.Core;
using Wordania.Features.Mapping;
using Wordania.Core.Inputs;
using Wordania.Features.Bosses.Core;
using Wordania.Features.Bosses.Data;
using Wordania.Core.Identifiers;
using Wordania.Features.World.Lighting;
using Wordania.Features.HUD.Journal;
using Wordania.Features.HUD.WeaponStore;
using Wordania.Features.Session;

namespace Wordania.Features
{
    public sealed class SessionEntryPoint : IAsyncStartable
    {
        private readonly ISaveService _save;
        private readonly SessionConfig _session;
        private readonly IWorldService _world;
        private readonly ISkyLightService _skyLightService;
        private readonly IStaticLightingService _staticLightingService;
        private readonly IWorldCollisionJobService _worldCollisionJob;
        public SessionEntryPoint(
            ISaveService saveService,
            SessionConfig session,
            IWorldService worldService,
            IWorldCollisionJobService worldCollisionJob,
            ISkyLightService skyLightService,
            IStaticLightingService staticLightingService
            )
        {
            _save = saveService;
            _session = session;
            _world = worldService;
            _worldCollisionJob = worldCollisionJob;
            _skyLightService = skyLightService;
            _staticLightingService = staticLightingService;
        }
        public async UniTask StartAsync(System.Threading.CancellationToken cancellation)
        {
            Debug.Log("<color=green>[SESSION] Session Start Sequence Initiated...</color>");

            if (_session.SaveSlot == 0)
            {
                _world.RandomizeSeed();
                await _world.GenerateWorldAsync(cancellation);
            }
            else
            {
                await _save.LoadGameAsync(_save.DefaultPrefix + _session.SaveSlot.ToString());
            }
            await _skyLightService.InitializeSkyLightAsync(cancellation, 5000);
            await _staticLightingService.InitializeLightAsync(cancellation);

            _worldCollisionJob.InitializeCollisionArray();
        }
    }
}
