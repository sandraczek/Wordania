using System.Collections.Generic;
using Wordania.Core.Identifiers;

namespace Wordania.Features.Journal
{
    public sealed class PlayerJournal : IPlayerJournal
    {
        private readonly int _playerId;
        private readonly Dictionary<AssetId, int> _enemies = new();
        private readonly Dictionary<AssetId, int> _bosses = new();
        private readonly Dictionary<AssetId, int> _blocks = new();

        public PlayerJournal(int playerId)
        {
            _playerId = playerId;
        }

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
        public void IncrementBlock(AssetId id)
        {
            if (_blocks.ContainsKey(id))
            {
                _blocks[id]++;
            }
            else
            {
                _blocks.Add(id, 1);
            }

        }
    }
}