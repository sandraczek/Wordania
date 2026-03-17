using UnityEngine;
using Wordania.Core.Gameplay;
using Wordania.Gameplay.Enemies.Data;

namespace Wordania.Gameplay.Enemies.Core
{
    public interface IEnemyFactory
    {
        IEnemy CreateEnemy(EnemyTemplate data, Vector3 position);
    }
}