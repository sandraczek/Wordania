using System;
using UnityEngine;
using VContainer.Unity;
using Wordania.Core.Combat.Events;
using Wordania.Core.Data;
using Wordania.Core.Events;
using Wordania.Core.Identifiers;
using Wordania.Features.Bosses.Data;
using Wordania.Features.Enemies.Data;

namespace Wordania.Features.Skills
{
    public class KillSkillPointService : IStartable, IDisposable
    {
        private readonly IAssetRegistry<EnemyTemplate> _enemyRegistry;
        private readonly IAssetRegistry<BossTemplate> _bossRegistry;
        private readonly ISkillTreeService _skills;
        private readonly IEventBusSession _eventBus;

        public KillSkillPointService(IAssetRegistry<EnemyTemplate> enemyRegistry, IAssetRegistry<BossTemplate> bossRegistry, ISkillTreeService skills, IEventBusSession eventBus)
        {
            _enemyRegistry = enemyRegistry;
            _bossRegistry = bossRegistry;
            _skills = skills;
            _eventBus = eventBus;
        }
        public void Start()
        {
            _eventBus.Subscribe<EnemyKillRecordedEvent>(HandleKill);
            _eventBus.Subscribe<BossKillRecordedEvent>(HandleBossKill);
        }
        public void Dispose()
        {
            _eventBus?.Unsubscribe<EnemyKillRecordedEvent>(HandleKill);
            _eventBus?.Unsubscribe<BossKillRecordedEvent>(HandleBossKill);
        }
        private void HandleKill(EnemyKillRecordedEvent e)
        {
            var template = _enemyRegistry.Get(e.EnemyId);
            if (template == null) return;

            AddReward(e.PersistentId, template.Reward, e.KillCount);
        }
        private void HandleBossKill(BossKillRecordedEvent e)
        {
            var template = _bossRegistry.Get(e.BossId);
            if (template == null) return;

            AddReward(e.PersistentId, template.Reward, e.KillCount);
        }
        private void AddReward(PersistentId persistentId, RewardData reward, int kills) // here choose player later
        {
            float mult = GetMultiplier(reward, kills);
            foreach (var skillPoint in reward.SkillPoints)
            {
                int points = Mathf.RoundToInt(skillPoint.Value * mult);
                if (points <= 0) continue;

                _skills.AddPoints(persistentId, skillPoint.Type, points);
            }
        }

        private float GetMultiplier(RewardData reward, int kills)
        {
            float res = 1;
            var thresholds = reward.SkillPointsThresholds;
            for (int i = 0; i < thresholds.Count; i++)
            {
                if (thresholds[i].KillsBefore >= kills) break;

                res = thresholds[i].Multiplier;
            }

            return res;
        }

    }
}