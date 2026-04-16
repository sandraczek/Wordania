using System.Threading;
using Cysharp.Threading.Tasks;

namespace Wordania.Features.World.Lighting
{
    public interface ISkyLightService
    {
        UniTask InitializeSkyLightAsync(CancellationToken token);
    }
}