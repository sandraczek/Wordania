namespace Wordania.Features.Mapping
{
    using UnityEngine;
    using VContainer;
    using Wordania.Core.HUD;
    using Wordania.Core.Inputs;

    [RequireComponent(typeof(CanvasGroup))]
    public class WorldMapDisplay : MonoBehaviour, IHUDWindow
    {
        private CanvasGroup _canvasGroup;

        private IInputReader _inputs;
        private IHUDStateManager _hud;
        private bool _isOpen = false;

        [Inject]
        public void Construct(IInputReader inputs, IHUDStateManager hudManager)
        {
            _inputs = inputs;
            _hud = hudManager;
        }
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
        private void Start()
        {
            _inputs.OnToggleMap += HandleMapToggle;
            _isOpen = false;
            ApplyVisibility(false);
        }
        private void OnDestroy()
        {
            if (_inputs != null)
            {
                _inputs.OnToggleMap -= HandleMapToggle;
            }
        }

        private void HandleMapToggle()
        {
            _isOpen = !_isOpen;

            if (_isOpen) _hud.RegisterOpenWindow(this);
            else _hud.UnregisterOpenWindow(this);

            ApplyVisibility(_isOpen);
        }
        public void Close()
        {
            if (!_isOpen) return;

            _hud.UnregisterOpenWindow(this);
            _isOpen = false;
            ApplyVisibility(false);
        }
        private void ApplyVisibility(bool open)
        {
            _canvasGroup.alpha = open ? 1f : 0f;
            _canvasGroup.interactable = open;
            _canvasGroup.blocksRaycasts = open;
        }
    }
}