using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Wordania.Features.Inventory
{
    public sealed class InventorySlotUI : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private TextMeshProUGUI _nameText;

        public void SetData(ItemData data, int count)
        {
            _icon.sprite = data.Icon;
            _icon.preserveAspect = true;
            _amountText.text = count > 1 ? count.ToString() : "";
            _nameText.text = data.DisplayName;
        }
    }
}