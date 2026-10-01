using System.Collections.Generic;
using UnityEngine;
using Wordania.Enemies.Data;
using Wordania.Journal.Entries;

namespace Wordania.HUD.Journal.EntryViews
{
    public sealed class JournalEnemyEntryView : JournalEntryView
    {
        public void SetData(JournalEnemyEntry entry, int killed)
        {
            base.SetData(entry, killed);
        }
    }
}