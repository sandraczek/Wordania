using UnityEngine;
using VContainer;
using Wordania.Core.SaveSystem;
using Wordania.Features.Session;

namespace Wordania.Features
{
    public sealed class DebugSaveComponent : MonoBehaviour
    {
        private ISaveService _saveService;
        private SessionConfig _sessionConfig;
        [Range(1, 9)]
        [SerializeField] private int _saveSlot = 1;

        [Inject]
        public void Construct(ISaveService saveService, SessionConfig sessionConfig)
        {
            _saveService = saveService;
            _sessionConfig = sessionConfig;
            if (sessionConfig.SaveSlot != 0)
                _saveSlot = sessionConfig.SaveSlot;
            else
                _saveSlot = 9;

            if (_saveService == null) Debug.LogError("save service is null");
        }

        [ContextMenu("Save")]
        public async void Save()
        {
            try
            {
                await _saveService.SaveGameAsync(_saveService.DefaultPrefix + _saveSlot.ToString());
            }
            catch
            {
                throw;
            }
        }
    }
}
