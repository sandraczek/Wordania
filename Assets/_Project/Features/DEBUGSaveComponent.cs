using UnityEngine;
using VContainer;
using Wordania.Core.SaveSystem;

namespace Wordania.Gameplay
{
    public class DebugSaveComponent : MonoBehaviour
    {
        private ISaveService _saveService;
        [Range(1,9)]
        [SerializeField] private int _saveSlot = 1;

        [Inject]
        public void Construct(ISaveService saveService)
        {
            _saveService = saveService;
            if(_saveService == null) Debug.LogError("save service is null");
        }

        [ContextMenu("Save")]
        public async void Save()
        {
            try
            {
                await _saveService.SaveGameAsync(_saveService.DefaultPrefix + _saveSlot.ToString());
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Save error: {ex.Message}");
            }
        }
    }
}
