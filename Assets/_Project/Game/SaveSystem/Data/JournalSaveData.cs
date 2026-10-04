using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Wordania.Constants;
using Wordania.Identifiers;

namespace Wordania.SaveSystem.Data
{
    [Serializable]
    public sealed class JournalSaveData
    {
        public PersistentId PersistentId;
        public JournalCategoryDto[] Categories = new JournalCategoryDto[(int)JournalCategory.COUNT];

    }

    [Serializable]
    public readonly struct JournalEntryDto
    {
        public readonly int Id;
        public readonly int Count;

        [JsonConstructor]
        public JournalEntryDto(int id, int count)
        {
            Id = id;
            Count = count;
        }
    }

    [Serializable]
    public sealed class JournalCategoryDto
    {
        public List<JournalEntryDto> Entries = new();
    }
}