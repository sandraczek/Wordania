using System;
using UnityEngine;
using Wordania.Bosses.Data;
using Wordania.Bosses.Data.SharedAttacks;

namespace Wordania.Bosses.Yeinn.Data
{
    [Serializable]
    public sealed class YeinnHeadData : BossPartData
    {
        [field: SerializeField] public SlamPlayerAttack Slam { get;private set; }
        [field: SerializeField] public ChasePlayerAttack Chase { get;private set; }
        [field: SerializeField] public HoverOverPlayerAttack Hover { get;private set; }
    }
    [Serializable]
    public sealed class YeinnHandData : BossPartData
    {
        [field: SerializeField] public SlamPlayerAttack Slam { get;private set; }
        [field: SerializeField] public SwipePlayerAttack Swipe { get;private set; }
        [field: SerializeField] public IdleReturnAttack Idle { get;private set; }
    }
}