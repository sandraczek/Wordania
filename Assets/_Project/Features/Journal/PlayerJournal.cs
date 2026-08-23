using System.Collections.Generic;
using UnityEditor.VersionControl;
using VContainer.Unity;
using Wordania.Core.Identifiers;
using Wordania.Features.Journal.Entries;
using Wordania.Features.World.Events;

namespace Wordania.Features.Journal
{
    public sealed class PlayerJournal : IPlayerJournal
    {
        private readonly InstanceId _playerId;
        private readonly Dictionary<AssetId, int>[] _entries;

        public PlayerJournal(InstanceId playerId)
        {
            _playerId = playerId;

            _entries = new Dictionary<AssetId, int>[(int)JournalCategory.COUNT];
            for (int i = 0; i < (int)JournalCategory.COUNT; i++)
            {
                _entries[i] = new(16);
            }
        }

        public int Increment(JournalCategory category, AssetId id)
        {
            int index = (int)category;
            if (_entries[index].ContainsKey(id))
            {
                _entries[index][id]++;
            }
            else
            {
                _entries[index].Add(id, 1);
            }
            return _entries[index][id];

        }
        public void IncrementBatch(JournalCategory category, IReadOnlyList<BlockMineRecord> minedBlocks)
        {
            int index = (int)category;

            for (int i = 0; i < minedBlocks.Count; i++)
            {
                var blocks = minedBlocks[i];

                if (_entries[index].ContainsKey(blocks.Id))
                {
                    _entries[index][blocks.Id] += blocks.Count;
                }
                else
                {
                    _entries[index].Add(blocks.Id, blocks.Count);
                }
            }
        }

        public IReadOnlyDictionary<AssetId, int> GetDictionary(JournalCategory category)
        {
            return _entries[(int)category];
        }
    }
}