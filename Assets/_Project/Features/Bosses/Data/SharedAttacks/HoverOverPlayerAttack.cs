using System;
using UnityEngine;

namespace Wordania.Bosses.Data.SharedAttacks
{
    [Serializable]
    public struct HoverOverPlayerAttack
    {
        public float Speed;
        public Vector2 VectorFromPlayer;
        public float MaxDistanceFromPlayer;
    }
}