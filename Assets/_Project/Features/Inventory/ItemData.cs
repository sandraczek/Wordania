using UnityEngine;

namespace Wordania.Gameplay.Inventory
{
    [CreateAssetMenu(fileName = "Item_New", menuName = "Inventory/Item")]
    public sealed class ItemData : ScriptableObject
    {
        [SerializeField] private string _id = "";
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private int _maxStackSize = 99;

        public string Id => _id;
        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public int MaxStackSize => _maxStackSize;
    }
}