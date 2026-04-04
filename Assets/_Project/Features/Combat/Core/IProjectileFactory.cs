

using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.Core.Combat;
using Wordania.Features.Combat.Data;

namespace Wordania.Features.Combat.Core
{
    public interface IProjectileFactory
    {
        void Get(ProjectileSpawnData spawnData);
        UniTask PrewarmPoolAsync(ProjectileData data);
    }
}