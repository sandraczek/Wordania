using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Wordania.Features.Skills;

namespace Wordania.Features.HUD.Skills
{
    public class SkillTreeView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _skillPointsText;
        [SerializeField] private Transform _nodesContainer;

        private SkillNodeView[] _nodeViews;

        public IReadOnlyCollection<SkillNodeView> NodeViews => _nodeViews;

        private void Awake()
        {
            _nodeViews = _nodesContainer.GetComponentsInChildren<SkillNodeView>(includeInactive: true);
        }

        public void UpdateSkillPoints(int[] currentPoints)
        {
            if (_skillPointsText != null)
            {
                string text = "";
                for (int i = 0; i < currentPoints.Length; i++)
                    text += $"{(SkillPointsType)i}: {currentPoints[i]}\n";

                _skillPointsText.text = text;
            }
        }
    }
}