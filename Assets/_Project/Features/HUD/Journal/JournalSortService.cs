using System;
using System.Collections.Generic;
using UnityEngine;
using Wordania.Features.Journal.Entries;

namespace Wordania.Features.HUD.Journal
{
    public sealed class JournalSortService : IJournalSortService
    {
        private readonly Dictionary<JournalSortType, Comparison<JournalEntry>> _strategies;

        public JournalSortService()
        {
            _strategies = new Dictionary<JournalSortType, Comparison<JournalEntry>>
            {
                { JournalSortType.Default, CompareBySortOrder },
                { JournalSortType.NameAscending, CompareByNameAsc },
                { JournalSortType.NameDescending, CompareByNameDesc }
            };
        }

        public void Sort<T>(List<T> list, JournalSortType type) where T : JournalEntry
        {
            if (_strategies.TryGetValue(type, out var comparisonFunction))
            {
                list.Sort(comparisonFunction);
            }
            else
            {
                Debug.LogWarning($"No sorting strategy for: {type}. List is unsorted.");
            }
        }

        private int CompareBySortOrder(JournalEntry x, JournalEntry y)
        => x.SortOrder.CompareTo(y.SortOrder);

        private int CompareByNameAsc(JournalEntry x, JournalEntry y)
            => string.Compare(x.DisplayName, y.DisplayName, StringComparison.Ordinal);

        private int CompareByNameDesc(JournalEntry x, JournalEntry y)
            => string.Compare(y.DisplayName, x.DisplayName, StringComparison.Ordinal);

    }
}