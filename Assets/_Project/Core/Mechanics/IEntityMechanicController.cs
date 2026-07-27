using Wordania.Core.Identifiers;

namespace Wordania.Core.Mechanics
{
    public interface IEntityMechanicController
    {
        void EnableMechanic(AssetId mechanicId, InstanceId source);
        void DisableMechanic(AssetId mechanicId, InstanceId source);
    }
}