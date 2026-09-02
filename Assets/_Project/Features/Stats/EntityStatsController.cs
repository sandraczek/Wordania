namespace Wordania.Features.Stats
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Wordania.Core.Gameplay;
    using Wordania.Core.Stats;

    public class StatsComponent : MonoBehaviour, IEntityStats
    {
        private CharacterStat[] _stats;
        private bool _statsInitialized = false;
        public void Initialize(IReadOnlyList<(StatType, float)> startingStats)
        {

            int maxIndex = -1;
            for (int i = 0; i < startingStats.Count; i++)
            {
                int index = (int)startingStats[i].Item1;
                if (index > maxIndex)
                {
                    maxIndex = index;
                }
            }

            _stats = new CharacterStat[maxIndex + 1];

            for (int i = 0; i < startingStats.Count; i++)
            {
                int statIndex = (int)startingStats[i].Item1;
                _stats[statIndex] = new CharacterStat(startingStats[i].Item2);
            }

            _statsInitialized = true;
        }

        public CharacterStat GetStat(StatType statType)
        {
            if (!_statsInitialized) Debug.LogWarning("Stats were not initialized.");

            int index = (int)statType;
            if (index >= 0 && index < _stats.Length)
            {
                return _stats[index];
            }

            return null;
        }

        public bool TryGetStat(StatType statType, out CharacterStat stat)
        {
            if (!_statsInitialized) Debug.LogWarning("Stats were not initialized.");

            stat = GetStat(statType);
            return stat != null;
        }

        public void InitializeSpawn()
        {
            foreach (var stat in _stats)
            {
                stat.RemoveAllModifiers();
            }
        }
    }
}