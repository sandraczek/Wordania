using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Wordania.Core.SaveSystem.Data;
using Cysharp.Threading.Tasks;
using System;

namespace Wordania.Core.SaveSystem
{
    public sealed class JsonSaveService : ISaveService
    {
        private readonly List<ISaveable> _saveables = new();

        public GameSaveData CurrentData { get; private set; } = new();
        public string DefaultPrefix => "Slot_";

        public event Action OnSavingStarted;
        public event Action OnSavingFinished;

        public void Register(ISaveable savable) => _saveables.Add(savable);
        public void Unregister(ISaveable savable) => _saveables.Remove(savable);

        public async UniTask SaveGameAsync(string slotName)
        {
            OnSavingStarted?.Invoke();
            CurrentData.LastPlayedDate = System.DateTime.Now.ToString("O");

            foreach (var saveable in _saveables)
            {
                saveable.CaptureState(CurrentData);
            }
            string path = GetSavePath(slotName);

            await UniTask.RunOnThreadPool(() =>
            {
                string json = JsonConvert.SerializeObject(CurrentData, Formatting.None);
                
                File.WriteAllText(path, json);
            });

            await UniTask.SwitchToMainThread();
            
            OnSavingFinished?.Invoke();
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