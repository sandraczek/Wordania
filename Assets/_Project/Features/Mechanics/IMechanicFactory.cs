using Wordania.Core.Identifiers;

namespace Wordania.Features.Mechanics
{
    public interface IMechanicFactory
    {
        public IMechanic CreateMechanic(AssetId mechanicId);
        public void ReleaseMechanic(AssetId mechanicId, IMechanic mechanic);
    }
}