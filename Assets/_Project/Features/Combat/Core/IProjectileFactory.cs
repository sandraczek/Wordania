

using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.Combat;
using Wordania.Combat.Data;
using Wordania.Combat.Events;

namespace Wordania.Combat.Core
{
    public interface IProjectileFactory
    {
        void Get(ProjectileFiredEvent firedEvent);
        UniTask PrewarmPoolAsync(ProjectileData data);
    }
}