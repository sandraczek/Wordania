using System.Collections.Generic;
using UnityEngine;
using Wordania.Journal.Entries;
using Wordania.World;

namespace Wordania.HUD.Journal
{
    public sealed class JournalBlockEntryView : JournalEntryView
    {
        public void SetData(JournalBlockEntry entry, int killed)
        {
            base.SetData(entry, killed);
        }
    }
}