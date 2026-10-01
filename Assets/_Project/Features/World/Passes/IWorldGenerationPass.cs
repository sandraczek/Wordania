using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.World.Data;

namespace Wordania.World.Passes
{
    public interface IWorldGenerationPass
    {
        UniTask Execute(CancellationToken token, WorldData data);
    }
}