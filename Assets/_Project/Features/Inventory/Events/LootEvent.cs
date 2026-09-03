using UnityEngine;
using System;
using Wordania.Features.Inventory;
using Wordania.Core.Events;
using Wordania.Core.Identifiers;

namespace Wordania.Features.Inventory.Events
{
    public readonly struct LootEvent : IGameEvent
    {
        public readonly InstanceId InstanceId;
        public readonly AssetId ItemId;
        public readonly int Quantity;

        public LootEvent(InstanceId instanceId, AssetId itemId, int quantity)
        {
            InstanceId = instanceId;
            ItemId = itemId;
            Quantity = quantity;
        }
    }
}