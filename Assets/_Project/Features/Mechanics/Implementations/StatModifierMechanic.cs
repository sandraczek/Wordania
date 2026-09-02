namespace Wordania.Features.Mechanics.Implementations
{
    using Wordania.Core.Gameplay;
    using Wordania.Core.Identifiers;
    using Wordania.Features.Mechanics;
    using Wordania.Core.Stats;
    using Wordania.Features.Stats;

    public class StatModifierMechanic : IMechanic
    {
        private readonly StatType _targetStat;
        private readonly float _value;
        private readonly StatModifierType _modifierType;

        private StatModifier _appliedModifier;
        private Entity _entity;

        public StatModifierMechanic(StatData data)
        {
            _targetStat = data.Stat;
            _value = data.Value;
            _modifierType = data.ModifierType;
        }

        public bool OnActivate(Entity entity)
        {
            _entity = entity;

            if (!entity.TryGetFeature<StatsComponent>(out var statsController))
            {
                return false;
            }


            if (!statsController.TryGetStat(_targetStat, out CharacterStat statToModify))
            {
                return false;
            }

            _appliedModifier = new StatModifier(_value, _modifierType);
            statToModify.AddModifier(_appliedModifier);

            return true;
        }

        public void OnDeactivate()
        {
            if (_appliedModifier != null && _entity != null && _entity.TryGetFeature<StatsComponent>(out var statsController))
            {
                CharacterStat statToModify = statsController.GetStat(_targetStat);

                statToModify?.RemoveModifier(_appliedModifier);
            }

            _appliedModifier = null;
            _entity = null;
        }
    }
}