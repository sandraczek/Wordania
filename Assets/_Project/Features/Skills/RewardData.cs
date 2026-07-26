using System;
using System.Collections.Generic;
using UnityEngine;
using Wordania.Features.Skills;

namespace Wordania.Features.Skills
{
    /// <summary>
    /// Class for registering rewards for killing
    /// Used both for enemies and bosses
    /// </summary>
    [Serializable]
    public sealed class RewardData
    {
        [SerializeField]
        private List<SkillPoint> _skillPoints = new();
        public IReadOnlyList<SkillPoint> SkillPoints => _skillPoints;

        [SerializeField]
        private List<KillSkillPointThreshold> _skillPointsThresholds = new();
        public IReadOnlyList<KillSkillPointThreshold> SkillPointsThresholds => _skillPointsThresholds;


#if UNITY_EDITOR
        public void EditorSortThreshold()
        {
            if (_skillPointsThresholds == null || _skillPointsThresholds.Count <= 1) return;

            _skillPointsThresholds.Sort((a, b) => a.KillsBefore.CompareTo(b.KillsBefore));
        }
#endif

    }
}