using UnityEngine;
using System;
using Wordania.Features.Inventory;
using Wordania.Core.Events;

namespace Wordania.Features.Inventory.Events
{
    public struct LootEvent : IGameEvent
    {
        public LootEvent(ItemData item, int quantity)
        {
            Item = item;
            Quantity = quantity;
        }
        public ItemData Item;
        public int Quantity;
    }
}