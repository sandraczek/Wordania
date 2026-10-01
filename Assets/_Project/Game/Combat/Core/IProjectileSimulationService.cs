using System;
using Wordania.Combat.Data;

namespace Wordania.Combat.Core
{

    public interface IProjectileSimulationService
    {
        event Action<ProjectileView> OnProjectileDeath;
        void Register(ref ProjectileRuntimeData runtimeData, ProjectileView view);
    }
}