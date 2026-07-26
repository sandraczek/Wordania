using UnityEngine;
using Wordania.Features.World;

namespace Wordania.Features.Journal.Entries
{
    [CreateAssetMenu(fileName = "NewBlockEntry", menuName = "Journal/Block")]
    public sealed class JournalBlockEntry : JournalEntry<BlockData>
    {
        public override JournalCategory Category => JournalCategory.Blocks;
    }
}