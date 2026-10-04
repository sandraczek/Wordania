using System;
using Wordania.Identifiers;

namespace Wordania.SaveSystem.Data
{
    [Serializable]
    public sealed class PlayerSaveData
    {
        public PersistentId PersistentId;
        public float[] Position = new float[3];
        public float CurrentHealth;
    }
}