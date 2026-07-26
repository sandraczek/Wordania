using Wordania.Core.Identifiers;

namespace Wordania.Features.Journal
{
    public interface IPlayerJournal
    {
        int IncrementEnemy(AssetId id);
        int IncrementBoss(AssetId id);
        int IncrementBlock(AssetId id);
    }
}