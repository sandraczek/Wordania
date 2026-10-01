using System.Collections.Generic;
using UnityEngine;
using Wordania.Data;
using Wordania.Identifiers;

namespace Wordania.Journal.Entries
{
    [CreateAssetMenu(fileName = "JournalEntryRegistry", menuName = "Journal/Registry")]
    public sealed class JournalEntryRegistry : AssetRegistry<JournalEntry>
    {
        protected override AssetId GetKey(JournalEntry entry) => entry.TargetId;
    }
}