using System.Collections.Generic;
using UnityEngine;
using Wordania.Features.Bosses.Data;
using Wordania.Features.Journal.Entries;
using Wordania.Features.Journal.Milestones;

namespace Wordania.Features.HUD.Journal
{
    public sealed class JournalBossEntryView : JournalEntryView
    {
        public void SetData(JournalBossEntry entry, int killed)
        {
            base.SetData(entry, killed);
        }
    }
}