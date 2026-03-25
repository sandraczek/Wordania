using UnityEngine;
using Wordania.Gameplay.Enemies.Data;

namespace Wordania.Gameplay.Enemies.Spawning
{
    public interface ISpawnValidator
    {
        bool IsValid(in EnemyTemplate template, Vector2 position);
    }
}