using System.Collections.Generic;
using UnityEngine;
using Wordania.Journal.Entries;

namespace Wordania.HUD.Journal.EntryViews
{
    public sealed class JournalBlockEntryView : JournalEntryView
    {
        public void SetData(JournalBlockEntry entry, int killed)
        {
            base.SetData(entry, killed);
        }
    }
}