using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Wordania.Core.SaveSystem.Data;

namespace Wordania.Core.SaveSystem
{
    public interface ISaveService
    {
        GameSaveData CurrentData { get; }
        string DefaultPrefix {get;}
        
        UniTask SaveGameAsync(string slotName);
        UniTask LoadGameAsync(string slotName);
        
        void Register(ISaveable savable);
        void Unregister(ISaveable savable);
    }
}