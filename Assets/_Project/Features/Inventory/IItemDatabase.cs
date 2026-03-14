using System;
using UnityEngine;

namespace Wordania.Gameplay.Inventory
{
    public interface IItemDatabase
    {
        ItemData GetItem(string id);
    }
}