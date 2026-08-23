using System.Collections.Generic;
using UnityEngine;
using Wordania.Features.Journal.Entries;
using Wordania.Features.Journal.Milestones;
using Wordania.Features.World;

namespace Wordania.Features.HUD.Journal
{
    public sealed class JournalBlockEntryView : JournalEntryView
    {
        public void SetData(JournalBlockEntry entry, int killed)
        {
            base.SetData(entry, killed);
        }
    }
}