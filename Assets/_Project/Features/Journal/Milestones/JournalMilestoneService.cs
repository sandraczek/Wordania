using System;
using VContainer.Unity;
using Wordania.Core.Combat.Events;
using Wordania.Core.Data;
using Wordania.Core.Events;
using Wordania.Core.Gameplay;
using Wordania.Features.Bosses.Data;
using Wordania.Features.Enemies.Data;
using Wordania.Features.Journal.Entries;

namespace Wordania.Features.Journal.Milestones
{
    public class JournalMilestoneService : IStartable, IDisposable
    {
        private readonly IEventBusGameplay _eventBus;
        private readonly IAssetRegistry<JournalEntry> _entryRegistry;
        private readonly IPlayerProvider _player;
        public JournalMilestoneService(IEventBusGameplay eventBus, IAssetRegistry<JournalEntry> entryRegistry, IPlayerProvider player)
        {
            _eventBus = eventBus;
            _entryRegistry = entryRegistry;
            _player = player;
        }
        public void Start()
        {
            _eventBus.Subscribe<EnemyKillRecordedEvent>(HandleKill);
            _eventBus.Subscribe<BossKillRecordedEvent>(HandleBossKill);
        }
        public void Dispose()
        {
            _eventBus.Unsubscribe<EnemyKillRecordedEvent>(HandleKill);
            _eventBus.Unsubscribe<BossKillRecordedEvent>(HandleBossKill);
        }

        private void HandleKill(EnemyKillRecordedEvent e)
        {
            var entry = _entryRegistry.Get(e.EnemyId);
            if (entry == null) return;

            foreach (var milestone in entry.Milestones)
            {
                if (milestone.TargetThreshold == e.KillCount)
                {
                    _player.SkillContext.UnlockMechanic(milestone.Mechanic.Id);
                }
            }
        }
        private void HandleBossKill(BossKillRecordedEvent e)
        {
            var entry = _entryRegistry.Get(e.BossId);
            if (entry == null) return;

            foreach (var milestone in entry.Milestones)
            {
                if (milestone.TargetThreshold == e.KillCount)
                {
                    _player.SkillContext.UnlockMechanic(milestone.Mechanic.Id);
                }
            }
        }
    }
}