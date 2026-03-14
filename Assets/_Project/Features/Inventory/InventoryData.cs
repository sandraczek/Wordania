using UnityEngine;
using System;
using System.Collections.Generic;
using VContainer;

namespace Wordania.Gameplay.Inventory
{
    public class InventoryData
    {
        public readonly Dictionary<string, InventoryEntry> _content = new();
    }
}