using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.Core.SaveSystem.Data;

namespace Wordania.Core.SaveSystem
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