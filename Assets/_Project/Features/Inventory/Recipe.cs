using UnityEngine;
using System;

namespace Wordania.Gameplay.Inventory
{
    [Serializable]
    public struct Ingredient
    {
        public ItemData item;
        public int amount;
    }

    [CreateAssetMenu(menuName = "Inventory/Recipe")]
    public sealed class Recipe : ScriptableObject
    {
        [SerializeField] private Ingredient[] _requirements;

        public Ingredient[] Requirements => _requirements;
    }
}