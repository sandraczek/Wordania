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
        private IEntityContext _context;

        public StatModifierMechanic(StatData data)
        {
            _targetStat = data.Stat;
            _value = data.Value;
            _modifierType = data.ModifierType;
        }

        public bool OnActivate(IEntityContext context)
        {
            _context = context;

            if (!context.TryGetFeature<EntityStatsController>(out var statsController))
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
            if (_appliedModifier != null && _context != null && _context.TryGetFeature<EntityStatsController>(out var statsController))
            {
                CharacterStat statToModify = statsController.GetStat(_targetStat);

                statToModify?.RemoveModifier(_appliedModifier);
            }

            _appliedModifier = null;
            _context = null;
        }
    }
}