using System.Collections.Generic;
using Wordania.Journal.Entries;

namespace Wordania.HUD.Journal
{
    public interface IJournalSortService
    {
        void Sort<T>(List<T> list, JournalSortType type) where T : JournalEntry;
    }
}