using UnityEngine;
using Wordania.Gameplay.Enemies.Data;

namespace Wordania.Gameplay.Enemies.Spawning
{
    public class SpaceClearanceValidator : ISpawnValidator
    {
        public bool IsValid(in EnemyTemplate template, Vector2 position)
        {
            var hit = Physics2D.OverlapBox(
                position,
                template.Spawn.RequiredClearanceSize,
                0f,
                template.Movement.GroundLayer);

            return hit == null;  
        }
    }
}