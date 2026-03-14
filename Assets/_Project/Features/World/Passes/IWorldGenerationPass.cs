using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Wordania.Gameplay.World
{
    public interface IWorldGenerationPass 
    {
        UniTask Execute(CancellationToken token,  WorldData data);
    }
}