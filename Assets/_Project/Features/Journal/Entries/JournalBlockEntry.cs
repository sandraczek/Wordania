using UnityEngine;
using Wordania.World.Data;

namespace Wordania.Journal.Entries
{
    [CreateAssetMenu(fileName = "NewBlockEntry", menuName = "Journal/Block")]
    public sealed class JournalBlockEntry : JournalEntry<BlockData>
    {

    }
}