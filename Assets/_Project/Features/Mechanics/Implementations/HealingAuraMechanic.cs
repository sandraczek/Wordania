using Wordania.Combat;
using Wordania.Identifiers;
using Wordania.Mechanics.Data;

namespace Wordania.Mechanics.Implementations
{
    public class HealingAuraMechanic : ITickableMechanic
    {
        private readonly HealingAuraMechanicData _data;
        private HealthComponent _health;
        private float _timer = 0f;

        public HealingAuraMechanic(HealingAuraMechanicData data)
        {
            _data = data;
        }

        public bool OnActivate(Entity entity)
        {
            if (!entity.TryGetFeature(out _health))
            {
                ResetState();
                return false;
            }
            return true;
        }

        public void OnTick(float deltaTime)
        {
            if (_health == null) return;

            _timer += deltaTime;

            if (_timer >= _data.TickRate)
            {
                _health.ApplyHealing(_data.HealAmount);
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
            _health = null;
        }
    }
}