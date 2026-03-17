using Wordania.Core.Gameplay;

namespace Wordania.Gameplay.Enemies.Core
{
    public interface IEnemyRegistryService
    {
        void Register(IEnemy enemy);
        void Unregister(IEnemy enemy);
    }
}