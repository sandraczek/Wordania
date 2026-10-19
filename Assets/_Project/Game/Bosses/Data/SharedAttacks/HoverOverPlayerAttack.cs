using System;
using UnityEngine;
using Wordania.Combat;

namespace Wordania.Bosses.Data.SharedAttacks
{
    [Serializable]
    public struct HoverOverPlayerAttack
    {
        public ThreatLevels Threat;
        public float Speed;
        public Vector2 VectorFromPlayer;
        public float MaxDistanceFromPlayer;
    }
}