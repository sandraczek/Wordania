using UnityEngine;
using Wordania.Identifiers;

namespace Wordania.Combat.Authority
{
    /// <summary>
    /// "I want to shoot this weapon from here in this direction." Network-safe.
    /// Damage multiplier and target faction are intentionally absent - the host derives them from the shooter.
    /// </summary>
    public readonly struct FireRequest
    {
        public readonly InstanceId Shooter;
        public readonly AssetId Weapon;
        public readonly Vector2 Origin;
        public readonly Vector2 Direction;

        public FireRequest(InstanceId shooter, AssetId weapon, Vector2 origin, Vector2 direction)
        {
            Shooter = shooter;
            Weapon = weapon;
            Origin = origin;
            Direction = direction;
        }
    }
}
