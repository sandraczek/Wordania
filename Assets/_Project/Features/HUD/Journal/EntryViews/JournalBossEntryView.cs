using System.Collections.Generic;
using UnityEngine;
using Wordania.Bosses.Data;
using Wordania.Journal.Entries;

namespace Wordania.HUD.Journal
{
    public sealed class JournalBossEntryView : JournalEntryView
    {
        public void SetData(JournalBossEntry entry, int killed)
        {
            base.SetData(entry, killed);
        }
    }
}