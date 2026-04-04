using UnityEngine;
using Wordania.Core.Identifiers;

namespace Wordania.Features.Combat.Data
{
    public struct WeaponFireContext
    {
        public Vector2 position;
        public Vector2 direction;
        public float damageMultiplier;
        public int instigatorId;
        public EntityFaction TargetFactionMask;
    }
}