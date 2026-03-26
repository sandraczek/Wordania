using UnityEngine;
using Wordania.Features.Enemies.Data;

namespace Wordania.Features.Enemies.Movement
{
    public static class EnemyMovementSafetyUtility
    {
        private static readonly RaycastHit2D[] _raycastResults = new RaycastHit2D[1];

        public static bool IsPathSafe
            (
            Vector2 enemyCenter, 
            float facingDirection, 
            float distanceFromCenter,
            float depthFromCenter,
            LayerMask groundLayer
            )
        {
            Vector2 rayOrigin = new(
                enemyCenter.x + (distanceFromCenter * facingDirection),
                enemyCenter.y
            );

            //Debug.DrawRay(rayOrigin,Vector3.down * depthFromCenter);

            int hitCount = Physics2D.RaycastNonAlloc(
                rayOrigin, 
                Vector2.down, 
                _raycastResults, 
                depthFromCenter,
                groundLayer
            );

            return hitCount > 0;
        }
    }
}