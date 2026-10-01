using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.Gameplay;
using Wordania.Enemies.Data;

namespace Wordania.Enemies.Core
{
    public interface IEnemyFactory
    {
        IEnemy CreateEnemy(EnemyTemplate data, Vector3 position);
        public UniTask PrewarmPoolAsync(EnemyTemplate template);
    }
}