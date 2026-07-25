using System;
using VContainer;
using VContainer.Unity;
using Wordania.Core.Inputs;

namespace Wordania.Core.HUD
{
    public class HUDStateManager : IHUDStateManager, IStartable, IDisposable
    {
        private readonly IInputReader _inputs;
        private IHUDWindow _activeWindow;

        [Inject]
        public HUDStateManager(IInputReader inputs)
        {
            _inputs = inputs;
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
            }
        }

        public void UnregisterOpenWindow(IHUDWindow window)
        {
            if (_activeWindow != window) return;

            _activeWindow = null;
            _inputs.SetGameplayMode();
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