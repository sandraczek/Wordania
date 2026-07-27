namespace Wordania.Core.Stats
{
    public interface IEntityStats
    {
        CharacterStat GetStat(StatType statType);
        bool TryGetStat(StatType statType, out CharacterStat stat);
    }
}