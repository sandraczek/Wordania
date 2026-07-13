// LightmapPresenter.cs
using System;
using VContainer.Unity;
using Wordania.Features.Events;
using Wordania.Features.World;
using Wordania.Features.World.Data;

namespace Wordania.Features.World.Lighting // lub Wordania.World.Presentation
{
    /// <summary>
    /// Binds the core lighting logic to the GPU rendering system.
    /// </summary>
    public class LightmapPresenter : IStartable, IDisposable, ILateTickable
    {
        private readonly IWorldService _world;
        private readonly IStaticLightingService _lightingService;
        private readonly ILightmapRenderer _lightmapRenderer;

        private readonly IEventBusGameplay _eventBus;

        private bool _isDirty = false;

        public LightmapPresenter
            (
            IWorldService world,
            IStaticLightingService lightingService,
            ILightmapRenderer lightmapRenderer,
            IEventBusGameplay eventBus
            )
        {
            _world = world;
            _lightingService = lightingService;
            _lightmapRenderer = lightmapRenderer;
            _eventBus = eventBus;
        }

        public void Start()
        {
            _eventBus.Subscribe<LightChangedEvent>(MarkAsDirty);
            MarkAsDirty(new());
        }

        private void MarkAsDirty(LightChangedEvent _)
        {
            _isDirty = true;
        }

        public void LateTick()
        {
            if (!_isDirty) return;
            if (_world.Data == null) return;

            _isDirty = false;

            _lightmapRenderer.ApplyLightmap(_world.Data.Tiles);
        }

        public void Dispose()
        {
            _eventBus?.Unsubscribe<LightChangedEvent>(MarkAsDirty);
        }
    }
}