using System;
using UnityEngine;
using VContainer;
using Wordania.Core.HUD;
using Wordania.Core.Inputs;

namespace Wordania.Core.HUD
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class HUDDisplay<T> : MonoBehaviour, IHUDWindow where T : MonoBehaviour
    {
        protected IInputReader _inputs;
        private IHUDStateManager _hud;

        protected T _view;
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
            _view = GetComponent<T>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            BindInputs();
            _isOpen = false;
            ApplyVisibility(false);
        }
        private void OnDestroy()
        {
            if (_inputs != null)
            {
                UnbindInputs();
            }
        }

        protected void HandleToggle()
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

            OnApplyVisibility(open);
        }
        protected abstract void OnApplyVisibility(bool open);

        protected abstract void BindInputs();
        protected abstract void UnbindInputs();
    }
}