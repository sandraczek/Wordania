using UnityEngine;
using System;
using Wordania.Inventory;
using Wordania.Events;
using Wordania.Identifiers;

namespace Wordania.Inventory.Events
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