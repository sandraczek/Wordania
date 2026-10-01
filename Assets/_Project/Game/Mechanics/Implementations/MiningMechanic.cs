using Wordania.Gameplay;
using Wordania.Identifiers;

namespace Wordania.Mechanics.Implementations
{
    public class MiningMechanic : IMechanic
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