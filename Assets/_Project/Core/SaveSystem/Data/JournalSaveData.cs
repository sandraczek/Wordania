using System;
using System.Collections.Generic;
using Wordania.Core.Constants;
using Wordania.Core.Identifiers;

namespace Wordania.Core.SaveSystem.Data
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