

using Cysharp.Threading.Tasks;

namespace Wordania.HUD.Loading
{
    public interface ILoadingScreenView
    {
        void Show();
        void UpdateProgress(float progress, string message = "");
        UniTask Hide();
    }
}