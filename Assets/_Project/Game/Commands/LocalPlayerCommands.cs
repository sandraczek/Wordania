using UnityEngine;
using Wordania.Data;
using Wordania.Identifiers;
using Wordania.Services;
using Wordania.Skills;
using Wordania.WeaponStore;

namespace Wordania.Commands
{
    /// <summary>Host/single-player implementation: validates and executes the request immediately.</summary>
    public sealed class LocalPlayerCommands : IPlayerCommands
    {
        private readonly IEntityRegistry _entities;
        private readonly ISkillTreeService _skills;
        private readonly IAssetRegistry<SkillData> _skillRegistry;
        private readonly IWeaponStoreService _store;

        public LocalPlayerCommands(
            IEntityRegistry entities,
            ISkillTreeService skills,
            IAssetRegistry<SkillData> skillRegistry,
            IWeaponStoreService store)
        {
            _entities = entities;
            _skills = skills;
            _skillRegistry = skillRegistry;
            _store = store;
        }

        public void RequestRevive(PersistentId player)
        {
            if (!_entities.TryGetPlayer(player, out var target)) return;
            if (!target.Context.Health.IsDead) return;

            target.Revive();
        }

        public void RequestUnlockSkill(PersistentId player, AssetId skillId)
        {
            SkillData skill = _skillRegistry.Get(skillId);
            if (!_skills.CanUnlock(player, skill)) return;

            _skills.UnlockSkill(player, skillId);
        }

        public void RequestBuyWeapon(PersistentId player, AssetId weaponId)
        {
            if (!_store.Buy(player, weaponId))
                Debug.Log("Requirements not met to buy that weapon.");
        }
    }
}
