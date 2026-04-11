using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.Features.World.Data;

namespace Wordania.Features.World
{
    public interface IWorldGenerator
    {
        public UniTask<WorldData> GenerateWorldAsync(CancellationToken token);
    }
}