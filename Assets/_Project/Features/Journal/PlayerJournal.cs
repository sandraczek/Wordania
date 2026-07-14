using System.Collections.Generic;
using Wordania.Core.Identifiers;

namespace Wordania.Features.Journal
{
    public sealed class PlayerJournal : IPlayerJournal
    {
        private readonly Dictionary<AssetId, int> _enemies = new();
        private readonly Dictionary<AssetId, int> _bosses = new();

        public void IncrementEnemy(AssetId id)
        {
            if (_enemies.ContainsKey(id))
            {
                _enemies[id]++;
            }
            else
            {
                _enemies.Add(id, 1);
            }

        }
        public void IncrementBoss(AssetId id)
        {
            if (_bosses.ContainsKey(id))
            {
                _bosses[id]++;
            }
            else
            {
                _bosses.Add(id, 1);
            }

        }
    }
}