using System.Collections.Generic;
using Wordania.Core.Gameplay;

namespace Wordania.Gameplay.Enemies.Core
{
    public sealed class EnemyRegistryService : IEnemyRegistryService
    {
        private readonly HashSet<IEnemy> _enemies = new();
        public void Register(IEnemy enemy)
        {
            _enemies.Add(enemy);
        }

        public void Unregister(IEnemy enemy)
        {
            _enemies.Remove(enemy);
        }
    }
}