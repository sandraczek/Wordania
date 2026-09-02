using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;
using Cysharp.Threading.Tasks;
using Wordania.Core.Services;
using Wordania.Features.Session;
using System.Threading.Tasks;
using System.Threading;
using Wordania.Core.Identifiers;

namespace Wordania.Features
{

    public class SessionBootstrapper : IAsyncStartable
    {
        private readonly ISceneLoaderService _sceneLoader;
        private LifetimeScope _currentSession;

        //debug
        private readonly DebugStartSettings _startSettings;

        public SessionBootstrapper(ISceneLoaderService sceneLoader, DebugStartSettings startSettings)
        {
            _sceneLoader = sceneLoader;
            _startSettings = startSettings;
        }

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            await StartNewGameAsync();
        }

        public async UniTask StartNewGameAsync()
        {
            var sessionGo = new GameObject("[SessionScope]");
            Object.DontDestroyOnLoad(sessionGo);

            _currentSession = sessionGo.AddComponent<SessionLifetimeScope>();
            _currentSession.autoRun = false;

            using (LifetimeScope.Enqueue(builder =>
            {
                builder.Register<SessionConfig>(Lifetime.Scoped)
                       .WithParameter(_startSettings.SaveSlot)
                       .WithParameter(true)
                       .WithParameter(PersistentId.New());
            }))
            {
                _currentSession.Build();
            }

            using (LifetimeScope.EnqueueParent(_currentSession))
            {
                await _sceneLoader.LoadWorldAsync();
            }
        }

        public void EndSession()
        {
            if (_currentSession != null)
            {
                Object.Destroy(_currentSession.gameObject);
                _currentSession = null;
            }

            _sceneLoader.LoadMenuAsync();
        }
    }
}