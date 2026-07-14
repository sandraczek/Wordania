using Wordania.Core.Identifiers;

namespace Wordania.Features.Journal
{
    public interface IPlayerJournal
    {
        void IncrementEnemy(AssetId id);
        void IncrementBoss(AssetId id);
    }
}