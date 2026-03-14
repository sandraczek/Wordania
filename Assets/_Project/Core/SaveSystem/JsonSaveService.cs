using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Wordania.Core.SaveSystem.Data;
using Cysharp.Threading.Tasks;

namespace Wordania.Core.SaveSystem
{
    public class JsonSaveService : ISaveService
    {
        private readonly List<ISaveable> _saveables = new();
        public GameSaveData CurrentData { get; private set; } = new();
        public string DefaultPrefix => "Slot_";


        public void Register(ISaveable savable) => _saveables.Add(savable);
        public void Unregister(ISaveable savable) => _saveables.Remove(savable);

        public async UniTask SaveGameAsync(string slotName)
        {
            Debug.Log($"Saving... ");
            CurrentData.LastPlayedDate = System.DateTime.Now.ToString("O");

            foreach (var saveable in _saveables)
            {
                saveable.CaptureState(CurrentData);
            }
            string path = GetSavePath(slotName);

            await UniTask.RunOnThreadPool(async () =>
            {
                string json = JsonConvert.SerializeObject(CurrentData, Formatting.None);
                
                await File.WriteAllTextAsync(path, json);
            });

            Debug.Log("Saved!");
        }

        public async UniTask LoadGameAsync(string slotName)
        {
            string path = GetSavePath(slotName);
            
            if (!File.Exists(path))
            {
                Debug.LogWarning("No Save File");
                CurrentData = new GameSaveData();
                return;
            }

            GameSaveData loadedData = await UniTask.RunOnThreadPool(async () =>
            {
                string json = await File.ReadAllTextAsync(path);
                return JsonConvert.DeserializeObject<GameSaveData>(json);
            });

            CurrentData = loadedData;

            foreach (var savable in _saveables)
            {
                savable.RestoreState(CurrentData);
            }
        }

        private string GetSavePath(string slotName)
        {
            return Path.Combine(Application.persistentDataPath, $"{slotName}.json");
        }
    }
}