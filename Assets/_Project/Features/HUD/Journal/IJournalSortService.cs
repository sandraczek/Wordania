using System.Collections.Generic;
using Wordania.Features.Journal.Entries;

namespace Wordania.Features.HUD.Journal
{
    public interface IJournalSortService
    {
        void Sort<T>(List<T> list, JournalSortType type) where T : JournalEntry;
    }
}