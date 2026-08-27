using Wordania.Core.SaveSystem.Data;

namespace Wordania.Core.SaveSystem
{
    public interface ISaveable
    {

        void CaptureState(GameSaveData saveData);

        void RestoreState(GameSaveData saveData);
    }
}