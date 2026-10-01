using UnityEngine;
using Wordania.Identifiers;

namespace Wordania.Combat.Data
{
    public struct WeaponFireContext
    {
        public Vector2 position;
        public Vector2 direction;
        public float damageMultiplier;
        public InstanceId instigatorId;
        public EntityFaction TargetFactionMask;
    }
}