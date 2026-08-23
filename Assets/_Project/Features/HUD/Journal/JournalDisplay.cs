using UnityEngine;
using VContainer;
using Wordania.Core.HUD;
using Wordania.Core.Inputs;

namespace Wordania.Features.HUD.Journal
{
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(JournalView))]
    public sealed class JournalDisplay : MonoBehaviour, IHUDWindow
    {
        private IInputReader _inputs;
        private IHUDStateManager _hud;

        private JournalView _view;
        private CanvasGroup _canvasGroup;
        private bool _isOpen = false;

        [Inject]
        public void Construct(IInputReader inputs, IHUDStateManager hudManager)
        {
            _inputs = inputs;
            _hud = hudManager;
        }

        private void Awake()
        {
            _view = GetComponent<JournalView>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            _inputs.OnToggleJournal += HandleJournalToggle;
            _isOpen = false;
            ApplyVisibility(false);
        }
        private void OnDestroy()
        {
            if (_inputs != null)
            {
                _inputs.OnToggleJournal -= HandleJournalToggle;
            }
        }

        private void HandleJournalToggle()
        {
            _isOpen = !_isOpen;

            if (_isOpen) _hud.RegisterOpenWindow(this);
            else _hud.UnregisterOpenWindow(this);

            ApplyVisibility(_isOpen);
        }
        public void Close()
        {
            if (!_isOpen) return;

            _isOpen = false;
            ApplyVisibility(false);
        }
        private void ApplyVisibility(bool open)
        {
            _canvasGroup.alpha = open ? 1f : 0f;
            _canvasGroup.interactable = open;
            _canvasGroup.blocksRaycasts = open;

            if (open)
                _view.LoadPage();
        }
    }
}