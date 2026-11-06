// LightmapPresenter.cs
using System;
using VContainer.Unity;
using Wordania.Events;
using Wordania.World.Config;

namespace Wordania.World.Lighting // lub Wordania.World.Presentation
{
    /// <summary>
    /// Binds the core lighting logic to the GPU rendering system.
    /// </summary>
    public class LightmapPresenter : IStartable, IDisposable, ILateTickable
    {
        private readonly IWorldService _world;
        private readonly IStaticLightingService _lightingService;
        private readonly ILightmapRenderer _lightmapRenderer;

        private readonly IEventBus _eventBus;

        private readonly WorldSettings _settings;

        private bool _isDirty = false;
        private float _appliedBrightness;
        private bool _appliedFullBright;

        public LightmapPresenter
            (
            IWorldService world,
            IStaticLightingService lightingService,
            ILightmapRenderer lightmapRenderer,
            IEventBus eventBus,
            WorldSettings settings
            )
        {
            _settings = settings;
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
            if (_settings.Brightness != _appliedBrightness || _settings.FullBright != _appliedFullBright)
            {
                _appliedBrightness = _settings.Brightness;
                _appliedFullBright = _settings.FullBright;
                _isDirty = true;
            }

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