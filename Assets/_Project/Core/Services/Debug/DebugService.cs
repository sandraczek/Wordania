using System;
using UnityEngine;
using VContainer.Unity;
using Wordania.Core.Config;
using Wordania.Core.Inputs;

namespace Wordania.Core.Services
{
    public sealed class DebugService : IDebugService, IStartable, IDisposable
    {
        private readonly IInputReader _inputReader;
        private readonly DebugSettings _settings;

        private bool _showChunks = false;
        private bool _isGodModeActive = false;
        public bool IsGodModeActive => _isGodModeActive;
        public bool IsShowChunksActive => _showChunks;

        public event Action<bool> OnShowChunksChanged;
        public event Action<bool> OnGodModeChanged;

        public DebugService(IInputReader inputReader, DebugSettings settings)
        {
            _inputReader = inputReader;
            _settings = settings;
        }

        public void Start()
        {
            _inputReader.OnToggleChunks += HandleToggleChunks;
            _inputReader.OnToggleGodMode += ToggleGodMode;
        }

        public void Dispose()
        {
            _inputReader.OnToggleChunks -= HandleToggleChunks;
            _inputReader.OnToggleGodMode -= ToggleGodMode;
        }

        private void HandleToggleChunks()
        {
            _showChunks = !_showChunks;
            OnShowChunksChanged?.Invoke(_showChunks);
            UnityEngine.Debug.Log($"<color=yellow>[Debug]</color> Show Chunks: {_showChunks}");
        }
        public void LogInformation(string message)
        {
            UnityEngine.Debug.Log($"<color=#4AF626>[SYSTEM]:</color> {message}");
        }

        public void ToggleGodMode()
        {
            _isGodModeActive = !_isGodModeActive;
            OnGodModeChanged?.Invoke(_isGodModeActive);
        }
    }

}