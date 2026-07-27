using Wordania.Core.Combat;
using Wordania.Core.Gameplay;
using Wordania.Features.Combat;
using Wordania.Features.Mechanics.Data;

namespace Wordania.Features.Mechanics.Implementations
{
    public class GodModeMechanic : IMechanic
    {
        //private readonly GodModeMechanicData _data;
        private InvincibilityController _invincibility;

        public GodModeMechanic(GodModeMechanicData data)
        {
            //_data = data;
        }

        public bool OnActivate(IEntityContext context)
        {
            if (!context.TryGetFeature(out _invincibility))
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