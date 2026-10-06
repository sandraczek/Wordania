using VContainer;
using Wordania.Combat;
using Wordania.Combat.Authority;
using Wordania.Gameplay;
using Wordania.Identifiers;
using Wordania.Mechanics.Data;

namespace Wordania.Mechanics.Implementations
{
    public class HealingAuraMechanic : ITickableMechanic
    {
        private readonly HealingAuraMechanicData _data;
        private ICombatAuthority _combat;
        private ITrackable _target;
        private float _timer = 0f;

        public HealingAuraMechanic(HealingAuraMechanicData data)
        {
            _data = data;
        }

        [Inject]
        public void Construct(ICombatAuthority combat)
        {
            _combat = combat;
        }

        public bool OnActivate(Entity entity)
        {
            if (!entity.TryGetFeature(out HealthComponent _) || !entity.TryGetFeature(out _target))
            {
                ResetState();
                return false;
            }
            return true;
        }

        public void OnTick(float deltaTime)
        {
            if (_target == null) return;

            _timer += deltaTime;

            if (_timer >= _data.TickRate)
            {
                _combat.RequestHeal(_target.InstanceId, _data.HealAmount);
                _timer -= _data.TickRate;
            }
        }

        public void OnDeactivate()
        {
            ResetState();
        }

        private void ResetState()
        {
            _timer = 0f;
            _target = null;
        }
    }
}