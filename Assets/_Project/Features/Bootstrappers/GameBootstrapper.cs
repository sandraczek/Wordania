using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;
using Wordania.Core.Services;

namespace Wordania.Boot
{
    public sealed class GameBootstrapper : IStartable
    {
        private readonly ISceneLoaderService _sceneLoader;

        public GameBootstrapper(ISceneLoaderService sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }

        public void Start()
        {
            RunGameSequence().Forget();
        }

        private async UniTaskVoid RunGameSequence()
        {

            string activeSceneName = SceneManager.GetActiveScene().name;

            if (activeSceneName != "Boot") 
            {
                Debug.Log($"<color=green>[BOOT] </color><color=yellow>Starting From Scene: {activeSceneName}. Skipping Boot Procedure.</color>");
                return;
            }

            Debug.Log($"<color=green>[BOOT] System Initialization Started...</color>");

            await UniTask.Delay(100); 

            Debug.Log($"<color=green>[BOOT] Initialization Complete. Loading Gameplay.</color>");
            await _sceneLoader.LoadGameplayAsync();
        }
    }
}