using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Wordania.Core.Services;

namespace Wordania.Boot.Services
{
    public sealed class SceneLoaderService : ISceneLoaderService
    {
        private const string SCENE_MENU = "MainMenu";
        private const string SCENE_GAMEPLAY = "Gameplay";

        public async UniTask LoadMenuAsync()
        {
            await SceneManager.LoadSceneAsync(SCENE_MENU).ToUniTask();
            // Here Loading Screen (Fade Out)
        }

        public async UniTask LoadGameplayAsync()
        {
            await SceneManager.LoadSceneAsync(SCENE_GAMEPLAY).ToUniTask();
        }
    }
}