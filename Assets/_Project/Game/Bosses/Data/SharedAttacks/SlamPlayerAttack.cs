using System;

namespace Wordania.Bosses.Data.SharedAttacks
{
    [Serializable]
    public struct SlamPlayerAttack
    {
        public float TimeToAttack;
        public float LiftHeight;
        public float SlamSpeed;
        public float MaxDistanceBelowDynamicPlayer;
        public float RecoveryDuration;
    }
}