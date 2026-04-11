using VContainer;
using VContainer.Unity;
using Wordania.Core.Services;
using Wordania.Boot.Services;
using Wordania.Core;
using System;
using UnityEngine;
using Wordania.Core.SaveSystem;
using Wordania.Core.Config;
using Wordania.Core.Inputs;

namespace Wordania.Boot
{
    public sealed class ProjectLifetimeScope : LifetimeScope
    {
        [SerializeField] private DebugSettings _debugSettings;
        [SerializeField] private InputReader _inputReader;
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<SceneLoaderService>(Lifetime.Singleton).As<ISceneLoaderService>();

            builder.RegisterInstance(_debugSettings);
            builder.RegisterEntryPoint<DebugService>(Lifetime.Singleton).As<IDebugService>();
            builder.RegisterInstance<IInputReader>(_inputReader);
            _inputReader.Initialize();
            builder.Register<JsonSaveService>(Lifetime.Singleton).As<ISaveService>();

            builder.RegisterEntryPoint<GameBootstrapper>();
        }
    }
}
