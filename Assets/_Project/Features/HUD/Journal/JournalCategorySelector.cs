using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wordania.Core.Constants;

namespace Wordania.Features.HUD.Journal
{
    public sealed class JournalCategorySelector : MonoBehaviour
    {
        [SerializeField] private Button _enemy;
        [SerializeField] private Button _boss;
        [SerializeField] private Button _block;

        private JournalView _view;

        private void Awake()
        {
            _view = GetComponentInParent<JournalView>();
        }

        private void Start()
        {
            if (_enemy != null)
                _enemy.onClick.AddListener(HandleEnemyClick);
            else Debug.LogWarning("[Journal Category Selector]: Enemy button is null");

            if (_boss != null)
                _boss.onClick.AddListener(HandleBossClick);
            else Debug.LogWarning("[Journal Category Selector]: Boss button is null");

            if (_block != null)
                _block.onClick.AddListener(HandleBlockClick);
            else Debug.LogWarning("[Journal Category Selector]: Block button is null");
        }
        private void OnDestroy()
        {
            if (_enemy != null)
                _enemy.onClick.RemoveAllListeners();
            if (_boss != null)
                _boss.onClick.RemoveAllListeners();
            if (_block != null)
                _block.onClick.RemoveAllListeners();
        }

        private void HandleEnemyClick() => _view.SwitchCategory(JournalCategory.Enemies);
        private void HandleBossClick() => _view.SwitchCategory(JournalCategory.Bosses);
        private void HandleBlockClick() => _view.SwitchCategory(JournalCategory.Blocks);
    }
}