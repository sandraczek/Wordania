using Wordania.Identifiers;

namespace Wordania.Mechanics
{
    public interface IMechanicFactory
    {
        public IMechanic CreateMechanic(AssetId mechanicId);
        public void ReleaseMechanic(AssetId mechanicId, IMechanic mechanic);
    }
}