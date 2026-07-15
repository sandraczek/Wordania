using Wordania.Core.Combat;
using Wordania.Core.Gameplay;
using Wordania.Features.Mechanics.Data;

namespace Wordania.Features.Mechanics.Implementations
{
    public class MiningMechanic : IMechanic
    {
        public bool OnActivate(IEntityContext context)
        {
            return true;
        }

        public void OnTick(float deltaTime)
        {

        }

        public void OnDeactivate()
        {

        }
    }
}