using UnityEditor;
using UnityEngine;
using Wordania.Core.Data;

namespace Wordania.Features.Skills
{
    [CreateAssetMenu(fileName = "SkillRegistry", menuName = "Skills/Registry")]
    public sealed class SkillRegistry : AssetRegistry<SkillData>
    {

    }
}