using UnityEngine;
using Wordania.Identifiers;

namespace Wordania.World.Editing
{
    public readonly struct MineRequest
    {
        public readonly InstanceId Instigator;
        public readonly Vector2 Position;
        public readonly float Power;
        public readonly bool IsArea;
        public readonly float Radius;

        public MineRequest(InstanceId instigator, Vector2 position, float power, bool isArea, float radius)
        {
            Instigator = instigator;
            Position = position;
            Power = power;
            IsArea = isArea;
            Radius = radius;
        }
    }

    public readonly struct PlaceRequest
    {
        public readonly InstanceId Instigator;
        public readonly PersistentId Owner; // TODO(net): host should derive it from the sender connection, not trust the client
        public readonly Vector2 Position;
        public readonly AssetId Block;

        public PlaceRequest(InstanceId instigator, PersistentId owner, Vector2 position, AssetId block)
        {
            Instigator = instigator;
            Owner = owner;
            Position = position;
            Block = block;
        }
    }
}
