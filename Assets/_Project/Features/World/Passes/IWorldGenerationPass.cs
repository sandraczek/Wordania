using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.Features.World.Data;

namespace Wordania.Features.World
{
    public interface IWorldGenerationPass
    {
        UniTask Execute(CancellationToken token, WorldData data);
    }
}