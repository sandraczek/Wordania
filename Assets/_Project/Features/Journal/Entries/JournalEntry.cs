using System.Collections.Generic;
using UnityEngine;
using Wordania.Core.Data;
using Wordania.Core.Identifiers;


namespace Wordania.Features.Journal.Entries
{
    public abstract class JournalEntry : DataAsset
    {
        public List<JournalMilestone> Milestones;

        public abstract JournalCategory Category { get; }
    }

    public enum JournalCategory
    {
        Enemies,
        Bosses,
        Blocks
    }
}