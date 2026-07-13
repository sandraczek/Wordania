

using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.Core.Combat;
using Wordania.Features.Combat.Data;
using Wordania.Features.Combat.Events;

namespace Wordania.Features.Combat.Core
{
    public interface IProjectileFactory
    {
        void Get(ProjectileFiredEvent firedEvent);
        UniTask PrewarmPoolAsync(ProjectileData data);
    }
}