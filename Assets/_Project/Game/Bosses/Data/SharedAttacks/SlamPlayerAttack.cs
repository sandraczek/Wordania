using System;
using Wordania.Combat;

namespace Wordania.Bosses.Data.SharedAttacks
{
    [Serializable]
    public struct SlamPlayerAttack
    {
        public ThreatLevels Threat;
        public float TimeToAttack;
        public float LiftHeight;
        public float SlamSpeed;
        public float MaxDistanceBelowDynamicPlayer;
        public float RecoveryDuration;
    }
}