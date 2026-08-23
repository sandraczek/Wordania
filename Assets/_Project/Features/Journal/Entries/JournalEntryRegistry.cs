using System.Collections.Generic;
using UnityEngine;
using Wordania.Core.Data;
using Wordania.Core.Identifiers;

namespace Wordania.Features.Journal.Entries
{
    [CreateAssetMenu(fileName = "JournalEntryRegistry", menuName = "Journal/Registry")]
    public sealed class JournalEntryRegistry : AssetRegistry<JournalEntry>
    {
        protected override AssetId GetKey(JournalEntry entry) => entry.TargetId;
    }
}