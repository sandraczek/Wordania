using VContainer;
using VContainer.Unity;
using Wordania.Core.Services;
using Wordania.Boot.Services;
using Wordania.Core;
using System;
using UnityEngine;
using Wordania.Core.SaveSystem;

namespace Wordania.Boot
{
    public class ProjectLifetimeScope : LifetimeScope
    {
        [SerializeField] private InputReader _inputReader;
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<SceneLoaderService>(Lifetime.Singleton).As<ISceneLoaderService>();
            builder.Register<DebugService>(Lifetime.Singleton).As<IDebugService>();
            builder.RegisterInstance<IInputReader>(_inputReader);
            _inputReader.Initialize();
            builder.Register<JsonSaveService>(Lifetime.Scoped).As<ISaveService>();

            builder.RegisterEntryPoint<GameBootstrapper>();
        }
    }
}
