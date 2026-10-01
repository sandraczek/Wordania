using System.Threading;
using Cysharp.Threading.Tasks;

namespace Wordania.World
{
    public interface IWorldRenderer
    {
        public UniTask RenderInitialWorldAsync(CancellationToken token);
    }
}