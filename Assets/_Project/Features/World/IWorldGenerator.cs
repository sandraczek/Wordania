using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.World.Data;

namespace Wordania.World
{
    public interface IWorldGenerator
    {
        public UniTask<WorldData> GenerateWorldAsync(CancellationToken token);
    }
}