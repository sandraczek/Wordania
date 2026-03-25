using UnityEngine;
using Wordania.Gameplay.Enemies.Config;
using Wordania.Gameplay.Enemies.Data;

namespace Wordania.Gameplay.Enemies.Spawning
{
    public class GroundCollisionValidator : ISpawnValidator
    {
        public bool IsValid(in EnemyTemplate template, Vector2 position)
        {
            if (!template.Spawn.RequiresGround)
            {
                return true;
            }
            Vector2 checkPosition = position + (template.Spawn.RequiredClearanceSize.y + 0.05f) * Vector2.down;

            Vector2 boxSize = new(template.Spawn.RequiredClearanceSize.x, 0.1f);

            var hit = Physics2D.OverlapBox(
                checkPosition,
                boxSize,
                0f,
                template.Movement.GroundLayer);

            return hit != null;
        }
    }
}