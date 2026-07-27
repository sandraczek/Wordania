using System;
using VContainer.Unity;
using Wordania.Core.Data;
using Wordania.Core.Identifiers;
using Wordania.Features.Skills;

namespace Wordania.Features.HUD.Skills
{
    public class SkillTreePresenter : IStartable, IDisposable
    {
        private readonly SkillTreeView _view;
        private readonly ISkillTreeService _skills;
        private readonly IAssetRegistry<SkillData> _skillRegistry;

        public SkillTreePresenter(
            SkillTreeView view,
            ISkillTreeService entitySkills,
            IAssetRegistry<SkillData> skillRegistry)
        {
            _view = view ? view : throw new ArgumentNullException(nameof(view));
            _skills = entitySkills ?? throw new ArgumentNullException(nameof(entitySkills));
            _skillRegistry = skillRegistry ?? throw new ArgumentNullException(nameof(skillRegistry));
        }

        public void Start()
        {
            _skills.OnSkillUnlocked += HandleSkillUnlocked;
            _skills.OnPointsChanged += HandlePointsChanged;

            foreach (var nodeView in _view.NodeViews)
            {
                nodeView.OnNodeClicked += HandleNodeClicked;

                SkillData data = _skillRegistry.Get(nodeView.Skill.Id);

                nodeView.Setup(data.Icon);
            }

            RefreshEntireTree();
        }

        public void Dispose()
        {
            _skills.OnSkillUnlocked -= HandleSkillUnlocked;
            _skills.OnPointsChanged -= HandlePointsChanged;

            foreach (var nodeView in _view.NodeViews)
            {
                nodeView.OnNodeClicked -= HandleNodeClicked;
            }
        }

        private void HandleNodeClicked(AssetId clickedSkillId)
        {
            if (_skills.IsSkillUnlocked(clickedSkillId))
            {
                return;
            }

            SkillData data = _skillRegistry.Get(clickedSkillId);
            if (_skills.CanUnlock(data))
            {
                _skills.UnlockSkill(clickedSkillId);
            }
            else
            {
                //UnityEngine.Debug.Log($"[SkillTreePresenter] Cannot unlock {clickedSkillId}. Requirements not met.");
            }
        }

        private void HandleSkillUnlocked(AssetId unlockedSkillId)
        {
            RefreshEntireTree();
        }

        private void HandlePointsChanged(int[] newPoints)
        {
            _view.UpdateSkillPoints(newPoints);

            RefreshEntireTree();
        }

        private void RefreshEntireTree()
        {
            _view.UpdateSkillPoints(_skills.SkillPoints);

            foreach (var nodeView in _view.NodeViews)
            {
                SkillData data = _skillRegistry.Get(nodeView.Skill.Id);

                SkillNodeState state = DetermineNodeState(data);
                nodeView.UpdateVisualState(state);
            }
        }

        private SkillNodeState DetermineNodeState(SkillData skillData)
        {
            if (_skills.IsSkillUnlocked(skillData.Id))
            {
                return SkillNodeState.Unlocked;
            }

            if (_skills.CanUnlock(skillData))
            {
                return SkillNodeState.Available;
            }

            return SkillNodeState.Locked;
        }
    }
}