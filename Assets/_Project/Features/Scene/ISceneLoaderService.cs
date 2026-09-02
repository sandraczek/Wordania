using Cysharp.Threading.Tasks;

namespace Wordania.Core.Services
{
    public interface ISceneLoaderService
    {
        UniTask LoadMenuAsync();
        UniTask LoadGameplayAsync();
        UniTask LoadWorldAsync();
    }
}