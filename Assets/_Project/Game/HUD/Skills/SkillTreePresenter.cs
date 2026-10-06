using System;
using VContainer.Unity;
using Wordania.Commands;
using Wordania.Data;
using Wordania.Identifiers;
using Wordania.Player;
using Wordania.Skills;

namespace Wordania.HUD.Skills
{
    public class SkillTreePresenter : IStartable, IDisposable
    {
        private readonly SkillTreeView _view;
        private readonly ISkillTreeService _skills;
        private readonly IAssetRegistry<SkillData> _skillRegistry;
        private readonly ILocalPlayer _local;
        private readonly IPlayerCommands _commands;

        public SkillTreePresenter(
            SkillTreeView view,
            ISkillTreeService entitySkills,
            IAssetRegistry<SkillData> skillRegistry,
            ILocalPlayer local,
            IPlayerCommands commands
            )
        {
            _view = view ? view : throw new ArgumentNullException(nameof(view));
            _skills = entitySkills ?? throw new ArgumentNullException(nameof(entitySkills));
            _skillRegistry = skillRegistry ?? throw new ArgumentNullException(nameof(skillRegistry));
            _local = local;
            _commands = commands;
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
            // Validation happens on the host; the UI only asks.
            _commands.RequestUnlockSkill(_local.PersistentId, clickedSkillId);
        }

        private void HandleSkillUnlocked(PersistentId player, AssetId unlockedSkillId)
        {
            if (!_local.Is(player)) return;

            RefreshEntireTree();
        }

        private void HandlePointsChanged(PersistentId player, int[] newPoints)
        {
            if (!_local.Is(player)) return;

            _view.UpdateSkillPoints(newPoints);

            RefreshEntireTree();
        }

        private void RefreshEntireTree()
        {
            _view.UpdateSkillPoints(_skills.GetSkillPoints(_local.PersistentId));

            foreach (var nodeView in _view.NodeViews)
            {
                SkillData data = _skillRegistry.Get(nodeView.Skill.Id);

                SkillNodeState state = DetermineNodeState(data);
                nodeView.UpdateVisualState(state);
            }
        }

        private SkillNodeState DetermineNodeState(SkillData skillData)
        {
            if (_skills.IsSkillUnlocked(_local.PersistentId, skillData.Id))
            {
                return SkillNodeState.Unlocked;
            }

            if (_skills.CanUnlock(_local.PersistentId, skillData))
            {
                return SkillNodeState.Available;
            }

            return SkillNodeState.Locked;
        }
    }
}