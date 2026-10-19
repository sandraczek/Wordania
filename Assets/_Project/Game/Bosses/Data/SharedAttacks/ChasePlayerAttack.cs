using System;
using Wordania.Combat;

namespace Wordania.Bosses.Data.SharedAttacks
{
    [Serializable]
    public struct ChasePlayerAttack
    {
        public ThreatLevels Threat;
        public float Speed;
    }
}