using System;
using UnityEngine;
using Wordania.Core.Config;
using Wordania.Core.Inputs;

namespace Wordania.Core.Services
{
    public sealed class DebugService : IDebugService
    {
        private readonly IInputReader _inputReader;
        private readonly DebugSettings _settings;

        public bool IsGodModeActive => _settings.GodMode;

        public event Action<bool> OnShowChunksChanged;

        public DebugService(IInputReader inputReader, DebugSettings settings)
        {
            _inputReader = inputReader;
            _settings = settings;
        }

        public void Initialize()
        {
            _inputReader.OnToggleChunks += HandleToggleChunks;
        }

        public void Dispose()
        {
            _inputReader.OnToggleChunks -= HandleToggleChunks;
        }

        private void HandleToggleChunks()
        {
            _settings.ShowChunks = !_settings.ShowChunks;
            OnShowChunksChanged?.Invoke(_settings.ShowChunks);
            UnityEngine.Debug.Log($"<color=yellow>[Debug]</color> Show Chunks: {_settings.ShowChunks}");
        }
        public void LogInformation(string message)
        {
            Debug.Log($"<color=#4AF626>[SYSTEM]:</color> {message}");
        }

        public void ToggleGodMode() => _settings.GodMode = !_settings.GodMode;
    }

}