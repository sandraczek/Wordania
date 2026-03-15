using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Wordania.Gameplay.Inventory{
    public sealed class InventorySlotUI : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private TextMeshProUGUI _nameText;

        public void SetData(InventoryEntry entry)
        {
            _icon.sprite = entry.Data.Icon;
            _icon.preserveAspect = true;
            _amountText.text = entry.Quantity > 1 ? entry.Quantity.ToString() : "";
            _nameText.text = entry.Data.DisplayName;
        }
    }
}