namespace Wordania.Features.Stats
{
    using System;
    using UnityEngine;
    using Wordania.Core.Gameplay;
    using Wordania.Core.Stats;

    public class EntityStatsController : MonoBehaviour, IEntityStats
    {
        [Header("Base Stat Definitions")]
        [SerializeField] private float _baseMaxHealth = 100f;
        [SerializeField] private float _baseMoveSpeed = 5f;
        [SerializeField] private float _baseAttackDamage = 10f;
        [SerializeField] private float _baseArmor = 0f;

        public CharacterStat MaxHealth { get; private set; }
        public CharacterStat MoveSpeed { get; private set; }
        public CharacterStat AttackDamage { get; private set; }
        public CharacterStat Armor { get; private set; }

        private CharacterStat[] _statsLookupTable;

        private void Awake()
        {
            InitializeStats();
        }

        private void InitializeStats()
        {
            MaxHealth = new CharacterStat(_baseMaxHealth);
            MoveSpeed = new CharacterStat(_baseMoveSpeed);
            AttackDamage = new CharacterStat(_baseAttackDamage);
            Armor = new CharacterStat(_baseArmor);

            int maxEnumIndex = 0;
            foreach (StatType statType in Enum.GetValues(typeof(StatType)))
            {
                int index = (int)statType;
                if (index > maxEnumIndex)
                {
                    maxEnumIndex = index;
                }
            }

            _statsLookupTable = new CharacterStat[maxEnumIndex + 1];

            _statsLookupTable[(int)StatType.MaxHealth] = MaxHealth;
            _statsLookupTable[(int)StatType.MoveSpeed] = MoveSpeed;
            _statsLookupTable[(int)StatType.AttackDamage] = AttackDamage;
            _statsLookupTable[(int)StatType.Armor] = Armor;
        }

        public CharacterStat GetStat(StatType statType)
        {
            int index = (int)statType;
            if (index >= 0 && index < _statsLookupTable.Length)
            {
                return _statsLookupTable[index];
            }

            return null;
        }

        public bool TryGetStat(StatType statType, out CharacterStat stat)
        {
            stat = GetStat(statType);
            return stat != null;
        }
    }
}