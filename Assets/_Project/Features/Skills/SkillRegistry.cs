using UnityEditor;
using UnityEngine;
using Wordania.Data;

namespace Wordania.Skills
{
    [CreateAssetMenu(fileName = "SkillRegistry", menuName = "Skills/Registry")]
    public sealed class SkillRegistry : AssetRegistry<SkillData>
    {

    }
}