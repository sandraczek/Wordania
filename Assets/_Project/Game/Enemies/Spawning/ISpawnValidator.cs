using UnityEngine;
using Wordania.Enemies.Data;

namespace Wordania.Enemies.Spawning
{
    public interface ISpawnValidator
    {
        bool IsValid(in EnemyTemplate template, Vector2 position);
    }
}