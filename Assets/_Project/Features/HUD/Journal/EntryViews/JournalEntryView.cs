using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Wordania.Features.Journal.Entries;
using Wordania.Features.Journal.Milestones;

namespace Wordania.Features.HUD.Journal
{
    public abstract class JournalEntryView : MonoBehaviour
    {
        private HUDConfig _config;

        protected bool _isUnlocked;

        [SerializeField] private Image _image;
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private TextMeshProUGUI _killCount;

        [HideInInspector] public Vector2Int PagePosition;

        [Inject]
        public void Construct(HUDConfig config)
        {
            _config = config;
        }
        protected void SetData(JournalEntry entry, int killed)
        {
            _isUnlocked = killed >= 1;

            if (_isUnlocked)
            {
                _name.text = entry.DisplayName;
                _image.sprite = entry.Icon;
            }
            else
            {
                _name.text = _config.JournalLockedName;
                _image.sprite = _config.JournalLockedSprite;
            }
            _killCount.text = killed.ToString();

        }
    }
}