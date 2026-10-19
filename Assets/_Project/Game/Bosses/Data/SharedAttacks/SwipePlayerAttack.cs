using System;
using Wordania.Combat;

namespace Wordania.Bosses.Data.SharedAttacks
{
    [Serializable]
    public struct SwipePlayerAttack
    {
        public ThreatLevels Threat;
        public float TimeToAttack;
        public float DistanceFromPlayer;
        public float AttackDistance;
        public float SwipeSpeed;
    }
}