using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.SaveSystem.Data;

namespace Wordania.SaveSystem
{
    public interface ISaveService
    {
        GameSaveData CurrentData { get; }
        string DefaultPrefix {get;}

        event Action OnSavingStarted;
        event Action OnSavingFinished;
        
        UniTask SaveGameAsync(string slotName);
        UniTask LoadGameAsync(string slotName);
        
        void Register(ISaveable savable);
        void Unregister(ISaveable savable);
    }
}