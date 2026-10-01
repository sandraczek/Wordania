using Wordania.Identifiers;

namespace Wordania.Mechanics
{
    public interface IEntityMechanicController
    {
        void EnableMechanic(AssetId mechanicId, InstanceId source);
        void DisableMechanic(AssetId mechanicId, InstanceId source);
    }
}