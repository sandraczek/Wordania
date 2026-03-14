using System.Threading;
using Cysharp.Threading.Tasks;

namespace Wordania.Gameplay.World
{
    public interface IWorldRenderer
    {
        public UniTask RenderInitialWorldAsync(CancellationToken token);
    }
}