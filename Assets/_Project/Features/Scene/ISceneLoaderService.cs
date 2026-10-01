using Cysharp.Threading.Tasks;

namespace Wordania.Services
{
    public interface ISceneLoaderService
    {
        UniTask LoadMenuAsync();
        UniTask LoadGameplayAsync();
        UniTask LoadWorldAsync();
    }
}