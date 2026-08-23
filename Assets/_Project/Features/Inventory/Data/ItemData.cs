using UnityEngine;
using Wordania.Core.Data;

namespace Wordania.Features.Inventory
{
    [CreateAssetMenu(fileName = "Item_New", menuName = "Inventory/Item")]
    public sealed class ItemData : DataAsset
    {
        [Space]
        [SerializeField] private string _displayName = "unnamed";
        [SerializeField] private Sprite _icon;
        [SerializeField] private int _maxStackSize = 99;
        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public int MaxStackSize => _maxStackSize;
    }
}