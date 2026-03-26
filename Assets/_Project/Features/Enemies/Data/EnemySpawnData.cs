using System;
using UnityEngine;

namespace Wordania.Features.Enemies.Data
{
    [Serializable]
    public class EnemySpawnData
    {
        [SerializeField] public bool RequiresGround = true;
        [SerializeField] public float MaxDistanceToGround = 1.5f;
        [SerializeField] public Vector2 RequiredClearanceSize;
        [SerializeField] public float RequiredGroundSize = 2f;
    }
}