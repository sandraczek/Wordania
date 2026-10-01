using Wordania.Combat;
using Wordania.Identifiers;
using Wordania.Mechanics.Data;

namespace Wordania.Mechanics.Implementations
{
    public class GodModeMechanic : IMechanic
    {
        //private readonly GodModeMechanicData _data;
        private InvincibilityController _invincibility;

        public GodModeMechanic(GodModeMechanicData data)
        {
            //_data = data;
        }

        public bool OnActivate(Entity entity)
        {
            if (!entity.TryGetFeature(out _invincibility))
            {
                return false;
            }

            _invincibility.SetInvincible(InvincibilitySource.GodMode, true);

            return true;
        }

        public void OnDeactivate()
        {
            _invincibility?.SetInvincible(InvincibilitySource.GodMode, false);
        }
    }
}