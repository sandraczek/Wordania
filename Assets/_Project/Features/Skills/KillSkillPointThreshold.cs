using System;

namespace Wordania.Skills
{
    [Serializable]
    public struct KillSkillPointThreshold
    {
        public int KillsBefore;
        public float Multiplier;
    }
}