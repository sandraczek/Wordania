

using Cysharp.Threading.Tasks;

namespace Wordania.Gameplay.HUD.Loading
{
    public interface ILoadingScreenService
    {
        void Show();
        void UpdateProgress(float progress, string message = "");
        UniTask Hide();
    }
}