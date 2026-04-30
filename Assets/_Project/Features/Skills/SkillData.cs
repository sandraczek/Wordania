using System.Collections.Generic;
using UnityEngine;
using Wordania.Core.Data;
using Wordania.Core.Identifiers;

namespace Wordania.Features.Skills
{
    [CreateAssetMenu(fileName = "NewSkillDefinition", menuName = "Skills/Data")]
    public class SkillData : DataAsset
    {
        [field: SerializeField] public string SkillName { get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public int Cost { get; private set; }

        [Tooltip("List of skill IDs required to unlock this skill.")]
        [field: SerializeField] public List<AssetId> Prerequisites { get; private set; } = new List<AssetId>();
    }
}