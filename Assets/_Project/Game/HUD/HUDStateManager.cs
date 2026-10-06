using System;
using VContainer;
using VContainer.Unity;
using Wordania.Inputs;
using Wordania.Services;

namespace Wordania.HUD
{
    public class HUDStateManager : IHUDStateManager, IStartable, IDisposable
    {
        private readonly IUIInput _inputs;
        private readonly IPauseService _pause;
        private IHUDWindow _activeWindow;

        [Inject]
        public HUDStateManager(IUIInput inputs, IPauseService pause)
        {
            _inputs = inputs;
            _pause = pause;
        }

        public void Start()
        {
            _inputs.OnExitPerformed += HandleExit;
        }
        public void Dispose()
        {
            if (_inputs != null)
                _inputs.OnExitPerformed -= HandleExit;

        }

        public void RegisterOpenWindow(IHUDWindow window)
        {
            if (_activeWindow == window) return;

            // Only one window may be open at a time; close whatever was open before.
            bool wasEmpty = _activeWindow == null;
            _activeWindow?.Close();
            _activeWindow = window;

            if (wasEmpty)
            {
                _inputs.SetHUDMode();
                _pause.SetPaused(true);
            }
        }

        public void UnregisterOpenWindow(IHUDWindow window)
        {
            if (window == null || _activeWindow != window) return;

            _activeWindow = null;
            _inputs.SetGameplayMode();
            _pause.SetPaused(false);
        }

        private void HandleExit()
        {
            if (_activeWindow == null)
            {
                // OPEN SETTINGS
            }
            else
            {
                _activeWindow.Close();
                UnregisterOpenWindow(_activeWindow);
            }
        }
    }
}