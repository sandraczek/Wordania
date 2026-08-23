using System.Collections.Generic;
using UnityEngine;
using Wordania.Features.Enemies.Data;
using Wordania.Features.Journal.Entries;
using Wordania.Features.Journal.Milestones;

namespace Wordania.Features.HUD.Journal
{
    public sealed class JournalEnemyEntryView : JournalEntryView
    {
        public void SetData(JournalEnemyEntry entry, int killed)
        {
            base.SetData(entry, killed);
        }
    }
}