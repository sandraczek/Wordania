using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Wordania.Core.Data;
using Wordania.Core.Identifiers;
using Wordania.Features.Combat.Data;

namespace Wordania.Features.HUD.WeaponStore
{
    public class WeaponStoreSlotView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private Image _image;
        [SerializeField] private Button _button;

        private AssetId _weaponId;

        [HideInInspector] public Vector2Int PagePosition;

        public event Action<AssetId> OnSlotClicked;

        public void SetData(WeaponData data)
        {
            _weaponId = data.Id;
            _name.text = data.name;
            _image.sprite = data.Icon;
        }

        private void Start()
        {
            _button.onClick.AddListener(HandleClicked);
        }
        private void OnDestroy()
        {
            _button.onClick.RemoveListener(HandleClicked);
        }

        private void HandleClicked()
        {
            OnSlotClicked?.Invoke(_weaponId);
        }
    }
}