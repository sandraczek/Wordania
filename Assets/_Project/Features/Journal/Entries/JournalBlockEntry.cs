using UnityEngine;
using Wordania.Features.World;

namespace Wordania.Features.Journal.Entries
{
    [CreateAssetMenu(fileName = "NewBlockEntry", menuName = "Journal/Block")]
    public sealed class JournalBlockEntry : JournalEntry
    {
        [SerializeField] private BlockData _block;

        public override JournalCategory Category => JournalCategory.Blocks;

    }
}