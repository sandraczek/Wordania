using Wordania.Core.Combat;
using Wordania.Core.Gameplay;
using Wordania.Core.Identifiers;
using Wordania.Features.Mechanics.Data;

namespace Wordania.Features.Mechanics.Implementations
{
    public class BuildingMechanic : IMechanic
    {
        public bool OnActivate(Entity entity)
        {
            return true;
        }

        public void OnDeactivate()
        {

        }
    }
}