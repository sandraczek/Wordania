using Wordania.Core.Identifiers;

namespace Wordania.Features.Player
{
    public interface IPlayerSkillContext
    {
        void UnlockMechanic(AssetId mechanicId);
        void AddModifier();
    }
}