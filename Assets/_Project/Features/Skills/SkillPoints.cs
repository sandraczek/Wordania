using System;

namespace Wordania.Features.Skills
{
    public enum SkillPointsType
    {
        Combat,
        Movement,
        Construction,

        Count
    }

    [Serializable]
    public struct SkillPoint
    {
        public SkillPointsType Type;
        public int Value;
    }
}