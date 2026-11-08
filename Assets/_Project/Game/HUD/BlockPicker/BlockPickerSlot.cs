using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wordania.Identifiers;

namespace Wordania.HUD.BlockPicker
{
    /// <summary>One cell of the block picker grid. Built in code by <see cref="BlockPickerView"/>.</summary>
    public sealed class BlockPickerSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private static readonly Color NormalColor = new(1f, 1f, 1f, 0.08f);
        private static readonly Color HoverColor = new(1f, 1f, 1f, 0.25f);
        private static readonly Color SelectedColor = new(1f, 0.8f, 0.2f, 0.85f);

        private Image _frame;
        private bool _hovered;
        private bool _selected;

        public AssetId Block { get; private set; }
        public string DisplayName { get; private set; }

        public event Action<BlockPickerSlot> Clicked;
        public event Action<BlockPickerSlot> HoverChanged;

        public bool IsHovered => _hovered;

        public void Setup(AssetId block, string displayName, Image frame)
        {
            Block = block;
            DisplayName = displayName;
            _frame = frame;
            Refresh();
        }

        public void SetSelected(bool selected)
        {
            _selected = selected;
            Refresh();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovered = true;
            Refresh();
            HoverChanged?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovered = false;
            Refresh();
            HoverChanged?.Invoke(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            Clicked?.Invoke(this);
        }

        private void OnDisable()
        {
            _hovered = false;
            Refresh();
        }

        private void Refresh()
        {
            if (_frame == null) return;
            _frame.color = _selected ? SelectedColor : _hovered ? HoverColor : NormalColor;
        }
    }
}
