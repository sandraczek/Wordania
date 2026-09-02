using System;
using VContainer.Unity;
using Wordania.Core.Data;
using Wordania.Core.Gameplay;
using Wordania.Core.Identifiers;
using Wordania.Features.Player;
using Wordania.Features.Skills;

namespace Wordania.Features.HUD.Skills
{
    public class SkillTreePresenter : IStartable, IDisposable
    {
        private readonly SkillTreeView _view;
        private readonly ISkillTreeService _skills;
        private readonly IAssetRegistry<SkillData> _skillRegistry;
        private readonly PlayerProvider _playerProvider;

        public SkillTreePresenter(
            SkillTreeView view,
            ISkillTreeService entitySkills,
            IAssetRegistry<SkillData> skillRegistry,
            PlayerProvider playerProvider
            )
        {
            _view = view ? view : throw new ArgumentNullException(nameof(view));
            _skills = entitySkills ?? throw new ArgumentNullException(nameof(entitySkills));
            _skillRegistry = skillRegistry ?? throw new ArgumentNullException(nameof(skillRegistry));
            _playerProvider = playerProvider;
        }

        public void Start()
        {
            _skills.OnLocalSkillUnlocked += HandleSkillUnlocked;
            _skills.OnLocalPointsChanged += HandlePointsChanged;

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
            _skills.OnLocalSkillUnlocked -= HandleSkillUnlocked;
            _skills.OnLocalPointsChanged -= HandlePointsChanged;

            foreach (var nodeView in _view.NodeViews)
            {
                nodeView.OnNodeClicked -= HandleNodeClicked;
            }
        }

        private void HandleNodeClicked(AssetId clickedSkillId)
        {
            if (_skills.IsSkillUnlocked(_playerProvider.PersistentId, clickedSkillId))
            {
                return;
            }

            SkillData data = _skillRegistry.Get(clickedSkillId);
            if (_skills.CanUnlock(_playerProvider.PersistentId, data))
            {
                _skills.UnlockSkill(_playerProvider.PersistentId, clickedSkillId);
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
            _view.UpdateSkillPoints(_skills.GetSkillPoints(_playerProvider.PersistentId));

            foreach (var nodeView in _view.NodeViews)
            {
                SkillData data = _skillRegistry.Get(nodeView.Skill.Id);

                SkillNodeState state = DetermineNodeState(data);
                nodeView.UpdateVisualState(state);
            }
        }

        private SkillNodeState DetermineNodeState(SkillData skillData)
        {
            if (_skills.IsSkillUnlocked(_playerProvider.PersistentId, skillData.Id))
            {
                return SkillNodeState.Unlocked;
            }

            if (_skills.CanUnlock(_playerProvider.PersistentId, skillData))
            {
                return SkillNodeState.Available;
            }

            return SkillNodeState.Locked;
        }
    }
}