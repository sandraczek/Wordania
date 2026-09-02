using System;
using System.Collections.Generic;
using Wordania.Core.Stats;

namespace Wordania.Core.SaveSystem.Data
{
    [Serializable]
    public sealed class PlayerSaveData
    {
        public string PersistentId;
        public float[] Position = new float[3];
        public float CurrentHealth;
    }
}