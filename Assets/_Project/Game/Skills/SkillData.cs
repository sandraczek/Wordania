using System;
using System.Collections.Generic;
using UnityEngine;
using Wordania.Attributes;
using Wordania.Data;
using Wordania.Mechanics.Data;
using Wordania.Stats;

namespace Wordania.Skills
{
    [CreateAssetMenu(fileName = "NewSkillDefinition", menuName = "Skills/Data")]
    public class SkillData : DataAsset
    {
        [field: SerializeField] public string SkillName { get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [SerializeField] private List<MechanicData> _mechanics = new();
        [SerializeField] private List<StatData> _stats = new();
        [field: SerializeField] public List<SkillPoint> Cost { get; private set; }

        public IReadOnlyList<MechanicData> Mechanics => _mechanics;
        public IReadOnlyList<StatData> Stats => _stats;

        [Tooltip("List of skill IDs required to unlock this skill.")]
        [field: SerializeField] public List<SkillData> Prerequisites { get; private set; } = new();
    }
}