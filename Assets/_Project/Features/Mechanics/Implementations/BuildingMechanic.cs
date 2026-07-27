using Wordania.Core.Combat;
using Wordania.Core.Gameplay;
using Wordania.Features.Mechanics.Data;

namespace Wordania.Features.Mechanics.Implementations
{
    public class BuildingMechanic : IMechanic
    {
        public bool OnActivate(IEntityContext context)
        {
            return true;
        }

        public void OnDeactivate()
        {

        }
    }
}