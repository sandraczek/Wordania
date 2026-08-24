using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;
using Wordania.Core.Data;
using Wordania.Core.Identifiers;
using Wordania.Features.Combat.Data;
using Wordania.Features.WeaponStore;

namespace Wordania.Features.HUD.WeaponStore
{
    public class WeaponStoreView : MonoBehaviour
    {
        private HUDConfig _config;
        private IObjectResolver _resolver;

        private readonly List<WeaponStoreSlotView> _slots = new();
        [SerializeField] private WeaponStoreSlotView _slotPrefab;
        [SerializeField] private GameObject _page;
        public event Action<AssetId> OnSlotClicked;

        private int _slotsNumberOnPage => _config.WeaponStoreSlotRectOnPage.x * _config.WeaponStoreSlotRectOnPage.y;

        [Inject]
        public void Construct(HUDConfig config, IObjectResolver resolver)
        {
            _config = config;
            _resolver = resolver;
        }

        private void OnDestroy()
        {
            ClearGrid();
        }

        public void SetData(IReadOnlyList<WeaponData> weapons)
        {
            if (weapons.Count > _slotsNumberOnPage) Debug.LogWarning("Time to add pages to WeaponStore, it is filled up.");
            int max = Mathf.Min(_slotsNumberOnPage, weapons.Count);
            for (int i = 0; i < max; i++)
            {
                _slots[i].gameObject.SetActive(true);
                _slots[i].SetData(weapons[i]);
            }
        }

        public void GenerateGrid()
        {
            var parentObj = new GameObject("WeaponSlots", typeof(RectTransform));
            var parentRect = parentObj.GetComponent<RectTransform>();

            parentRect.SetParent(_page == null ? transform : _page.transform, false);

            parentRect.anchorMin = Vector2.zero;
            parentRect.anchorMax = Vector2.one;
            parentRect.sizeDelta = Vector2.zero;
            parentRect.anchoredPosition = Vector2.zero;

            var gridLayout = parentObj.AddComponent<GridLayoutGroup>();

            Vector2 onPage = _config.WeaponStoreSlotRectOnPage;
            if (onPage.x <= 0 || onPage.y <= 0)
            {
                Debug.LogWarning("WeaponStore Slots on page is invalid. Using (1,1)");
                onPage = new Vector2(1, 1);
            }

            Canvas.ForceUpdateCanvases();

            Vector2 entrySize = new(
                parentRect.rect.width / onPage.x,
                parentRect.rect.height / onPage.y
            );

            gridLayout.cellSize = entrySize;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.spacing = Vector2.zero;

            _slots.Clear();
            for (int i = 0; i < _slotsNumberOnPage; i++)
            {
                Vector2Int gridPos = new(i % (int)onPage.x, i / (int)onPage.x);

                var view = _resolver.Instantiate(_slotPrefab);
                view.transform.SetParent(parentRect, false);

                view.gameObject.SetActive(false);
                view.OnSlotClicked += HandleSlotClicked;

                view.PagePosition = gridPos;

                _slots.Add(view);
            }
        }

        private void ClearGrid()
        {
            foreach (var slot in _slots)
            {
                slot.OnSlotClicked -= HandleSlotClicked;
                Destroy(slot.gameObject);
            }

            _slots.Clear();
        }

        private void HandleSlotClicked(AssetId id)
        {
            OnSlotClicked?.Invoke(id);
        }
    }
}