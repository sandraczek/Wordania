using Wordania.SaveSystem.Data;

namespace Wordania.SaveSystem
{
    public interface ISaveable
    {

        void CaptureState(GameSaveData saveData);

        void RestoreState(GameSaveData saveData);
    }
}