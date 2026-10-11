using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.SaveSystem.Data;

namespace Wordania.SaveSystem
{
    public interface ISaveService
    {
        GameSaveData CurrentData { get; }
        string DefaultPrefix { get; }

        event Action OnSavingStarted;
        event Action OnSavingFinished;

        UniTask SaveGameAsync(string slotName);
        UniTask LoadGameAsync(string slotName);

        /// <summary>Captures the current state of all saveables into new data, without touching disk or CurrentData.</summary>
        GameSaveData CaptureSnapshot();
        /// <summary>Makes <paramref name="data"/> current and restores all saveables from it (no disk access).</summary>
        void RestoreSnapshot(GameSaveData data);

        void Register(ISaveable savable);
        void Unregister(ISaveable savable);
    }
}