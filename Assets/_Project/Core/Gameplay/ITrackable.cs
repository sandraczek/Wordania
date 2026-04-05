using Unity.Mathematics;
using UnityEngine;
using Wordania.Core.Identifiers;

namespace Wordania.Core.Gameplay
{
    public struct TargetAABB
    {
        public int EntityInstanceId;
        public int FactionId;
        public float2 Min;           // Bottom-Left corner of the hitbox
        public float2 Max;           // Top-Right corner of the hitbox
    }
    public interface ITrackable : IEntity
    {
        Bounds Hitbox { get; }
        EntityFaction Faction {get;}
    }
}