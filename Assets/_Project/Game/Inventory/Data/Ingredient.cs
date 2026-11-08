using System;

namespace Wordania.Inventory.Data
{
    [Serializable]
    public sealed class Ingredient
    {
        public ItemData item;
        public int amount;
    }
}