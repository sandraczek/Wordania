using Cysharp.Threading.Tasks;

namespace Wordania.Scenes
{
    public interface ISceneLoaderService
    {
        UniTask LoadMenuAsync();
        UniTask LoadGameplayAsync();
        UniTask LoadWorldAsync();
    }
}