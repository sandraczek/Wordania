using System;

namespace Wordania.Bosses.Data.SharedAttacks
{
    [Serializable]
    public struct SwipePlayerAttack
    {
        public float TimeToAttack;
        public float DistanceFromPlayer;
        public float AttackDistance;
        public float SwipeSpeed;
    }
}