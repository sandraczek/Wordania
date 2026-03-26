using UnityEngine;
using Wordania.Features.Enemies.Config;
using Wordania.Features.Enemies.Data;

namespace Wordania.Features.Enemies.Spawning
{
    public class GroundCollisionValidator : ISpawnValidator
    {
        private readonly RaycastHit2D[] _raycastResults = new RaycastHit2D[1];
        
        // public bool IsValid(in EnemyTemplate template, Vector2 position)
        // {
        //     if (!template.Spawn.RequiresGround) return true;
            
        //     Vector2 checkPosition = position + (template.Spawn.RequiredClearanceSize.y + 0.05f) * Vector2.down;

        //     Vector2 boxSize = new(template.Spawn.RequiredClearanceSize.x, 0.1f);

        //     var hit = Physics2D.OverlapBox(
        //         checkPosition,
        //         boxSize,
        //         0f,
        //         template.Movement.GroundLayer);

        //     return hit != null;
        // }

        public bool IsValid(in EnemyTemplate template, Vector2 position)
        {
            if (!template.Spawn.RequiresGround) return true;

            float rayY = position.y - template.Spawn.RequiredClearanceSize.y / 2f;
            float rayX = position.x - template.Spawn.RequiredGroundSize / 2f;

            float maxX = position.x + template.Spawn.RequiredGroundSize / 2f;

            while (rayX < maxX)
            {
                if(!GroundFound(new(rayX,rayY), template.Spawn.MaxDistanceToGround, template.Movement.GroundLayer)) return false;

                rayX+=1f;
            }

            return GroundFound(new(maxX,rayY), template.Spawn.MaxDistanceToGround, template.Movement.GroundLayer);
        }

        private bool GroundFound(Vector2 rayPos, float depth, LayerMask groundLayer)
        {
            return Physics2D.RaycastNonAlloc(rayPos, Vector2.down, _raycastResults, depth, groundLayer) > 0;
        }
    }
}