using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Wordania.Identifiers;
using Wordania.Skills;

namespace Wordania.HUD.Skills
{
    public enum SkillNodeState
    {
        Locked,
        Available,
        Unlocked
    }

    public class SkillNodeView : MonoBehaviour, IPointerClickHandler
    {
        [field: SerializeField] public SkillData Skill { get; private set; }

        [Header("UI References")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _backgroundImage;

        [Header("Visual States")]
        [SerializeField] private Color _lockedColor = Color.gray;
        [SerializeField] private Color _availableColor = Color.white;
        [SerializeField] private Color _unlockedColor = Color.green;

        public event Action<AssetId> OnNodeClicked;

        public void Setup(Sprite icon)
        {
            if (_iconImage != null && icon != null)
            {
                _iconImage.sprite = icon;
            }
        }

        public void UpdateVisualState(SkillNodeState state)
        {
            if (_backgroundImage == null) return;

            switch (state)
            {
                case SkillNodeState.Locked:
                    _backgroundImage.color = _lockedColor;
                    break;
                case SkillNodeState.Available:
                    _backgroundImage.color = _availableColor;
                    break;
                case SkillNodeState.Unlocked:
                    _backgroundImage.color = _unlockedColor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnNodeClicked?.Invoke(Skill.Id);
        }
    }
}