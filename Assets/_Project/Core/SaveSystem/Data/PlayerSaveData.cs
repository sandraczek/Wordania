using System;

namespace Wordania.Core.SaveSystem.Data
{
    [Serializable]
    public sealed class PlayerSaveData
    {
        public float[] Position = new float[2];
        public float CurrentHealth;
        public float MaxHealth;
    }
}