using System;
using UnityEngine;

namespace Wordania.Bosses.Yeinn.Data
{
    [Serializable]
    public struct YeinnPhaseOneData
    {
        [Min(0f)] [field: SerializeField] public float InitialCooldown { get; private set; }
        [Min(0f)] [field: SerializeField] public float AttackInterval { get; private set; }
        [field: Range(0f, 1f)] [field: SerializeField] public float SwipeAttackProbability { get; private set; }
    }

    [Serializable]
    public struct YeinnPhaseTwoData
    {
        [Min(0f)] [field: SerializeField] public float InitialCooldown { get; private set; }
        [Min(0f)] [field: SerializeField] public float SlamInterval { get; private set; }
        [Range(0f,1f)] [field: SerializeField] public float HeadResistance { get; private set; }
    }
}