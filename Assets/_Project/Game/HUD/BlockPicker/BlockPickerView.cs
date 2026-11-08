using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Wordania.Identifiers;
using Wordania.World.Data;

namespace Wordania.HUD.BlockPicker
{
    /// <summary>
    /// Creative-style grid of blocks. Builds its own overlay canvas in code (no prefab/scene setup needed).
    /// Dumb view: reports clicks, the presenter decides what to do.
    /// </summary>
    public sealed class BlockPickerView : MonoBehaviour
    {
        private const int Columns = 8;
        private const float CellSize = 64f;
        private const float Spacing = 8f;
        private const float IconPadding = 8f;

        private readonly List<BlockPickerSlot> _slots = new();
        private GameObject _panel;
        private RectTransform _grid;
        private TMP_Text _title;
        private string _selectedName = string.Empty;

        public event Action<AssetId> BlockClicked;

        public static BlockPickerView Create()
        {
            var go = new GameObject("BlockPicker");
            return go.AddComponent<BlockPickerView>();
        }

        private void Awake()
        {
            Build();
            SetVisible(false);
        }

        public void SetVisible(bool visible) => _panel.SetActive(visible);

        public void Populate(IEnumerable<BlockData> blocks)
        {
            foreach (BlockPickerSlot slot in _slots)
                Destroy(slot.gameObject);
            _slots.Clear();

            foreach (BlockData block in blocks)
                _slots.Add(CreateSlot(block));
        }

        public void SetSelected(AssetId block)
        {
            _selectedName = string.Empty;
            foreach (BlockPickerSlot slot in _slots)
            {
                bool selected = slot.Block == block;
                slot.SetSelected(selected);
                if (selected) _selectedName = slot.DisplayName;
            }
            RefreshTitle(null);
        }

        private void Build()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            gameObject.AddComponent<GraphicRaycaster>();

            // Panel: centered, sized by its content.
            _panel = CreateUIObject("Panel", transform);
            var panelRect = (RectTransform)_panel.transform;
            panelRect.anchorMin = panelRect.anchorMax = panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            _panel.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.1f, 0.92f);

            var layout = _panel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 12, 16);
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var fitter = _panel.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Title
            GameObject titleGo = CreateUIObject("Title", _panel.transform);
            _title = titleGo.AddComponent<TextMeshProUGUI>();
            _title.fontSize = 28f;
            _title.alignment = TextAlignmentOptions.Center;
            _title.color = Color.white;
            _title.raycastTarget = false;

            // Grid
            GameObject gridGo = CreateUIObject("Grid", _panel.transform);
            _grid = (RectTransform)gridGo.transform;
            var grid = gridGo.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(CellSize, CellSize);
            grid.spacing = new Vector2(Spacing, Spacing);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = Columns;
            grid.childAlignment = TextAnchor.UpperLeft;

            RefreshTitle(null);
        }

        private BlockPickerSlot CreateSlot(BlockData block)
        {
            GameObject slotGo = CreateUIObject(block.name, _grid);
            var frame = slotGo.AddComponent<Image>(); // also the raycast target of the slot

            GameObject iconGo = CreateUIObject("Icon", slotGo.transform);
            var iconRect = (RectTransform)iconGo.transform;
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(IconPadding, IconPadding);
            iconRect.offsetMax = new Vector2(-IconPadding, -IconPadding);

            var icon = iconGo.AddComponent<Image>();
            icon.raycastTarget = false;
            icon.preserveAspect = true;
            if (block.Tile is Tile tile && tile.sprite != null)
            {
                icon.sprite = tile.sprite;
                icon.color = tile.color;
            }
            else
            {
                Color32 mapColor = block.MapColor;
                mapColor.a = 255;
                icon.color = mapColor;
            }

            var slot = slotGo.AddComponent<BlockPickerSlot>();
            slot.Setup(block.Id, DisplayNameOf(block), frame);
            slot.Clicked += HandleSlotClicked;
            slot.HoverChanged += HandleSlotHoverChanged;
            return slot;
        }

        private void HandleSlotClicked(BlockPickerSlot slot) => BlockClicked?.Invoke(slot.Block);

        private void HandleSlotHoverChanged(BlockPickerSlot slot) => RefreshTitle(slot.IsHovered ? slot : null);

        private void RefreshTitle(BlockPickerSlot hovered)
        {
            string name = hovered != null ? hovered.DisplayName : _selectedName;
            _title.text = string.IsNullOrEmpty(name) ? "Blocks" : $"Blocks - {name}";
        }

        private static string DisplayNameOf(BlockData block) => block.name.Replace("_Block", string.Empty);

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(parent, false);
            return go;
        }
    }
}
