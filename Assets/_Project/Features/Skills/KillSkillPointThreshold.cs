using System;

namespace Wordania.Features.Skills
{
    [Serializable]
    public struct KillSkillPointThreshold
    {
        public int KillsBefore;
        public float Multiplier;
    }
}