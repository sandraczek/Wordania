using UnityEngine;
using VContainer;
using Wordania.Core.SaveSystem;

namespace Wordania.Features
{
    public sealed class DebugSaveComponent : MonoBehaviour
    {
        private ISaveService _saveService;
        [Range(1, 9)]
        [SerializeField] private int _saveSlot = 1;

        [Inject]
        public void Construct(ISaveService saveService, int defaultSaveSlot)
        {
            _saveService = saveService;
            if (defaultSaveSlot != 0)
                _saveSlot = defaultSaveSlot;
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
