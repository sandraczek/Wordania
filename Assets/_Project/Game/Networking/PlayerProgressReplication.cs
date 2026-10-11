using System;
using System.Collections.Generic;
using VContainer.Unity;
using Wordania.Events;
using Wordania.Identifiers;
using Wordania.Inventory;
using Wordania.Journal;
using Wordania.SaveSystem.Data;
using Wordania.Services;
using Wordania.Session;
using Wordania.Skills;

namespace Wordania.Networking
{
    /// <summary>
    /// Host -> clients: full current state of the parts of one player's progress that changed. Null = unchanged.
    /// Uses the save formats, so the client restores it the same way a save is loaded.
    /// </summary>
    public readonly struct PlayerProgressEvent : IReplicatedEvent
    {
        public readonly PersistentId Player;
        public readonly InventorySaveData Inventory;
        public readonly SkillSaveData Skills;
        public readonly JournalSaveData Journal;

        public PlayerProgressEvent(PersistentId player, InventorySaveData inventory, SkillSaveData skills, JournalSaveData journal)
        {
            Player = player;
            Inventory = inventory;
            Skills = skills;
            Journal = journal;
        }
    }

    /// <summary>
    /// Host only: collects which players' inventory / skills / journal changed and sends their state at a fixed rate
    /// (mining changes the journal every frame, so changes are coalesced).
    /// </summary>
    public sealed class PlayerProgressReplicationHost : IStartable, ILateTickable, IDisposable
    {
        private const float SendInterval = 0.1f;

        [Flags]
        private enum Parts : byte
        {
            None = 0,
            Inventory = 1 << 0,
            Skills = 1 << 1,
            Journal = 1 << 2,
        }

        private readonly IInventoryService _inventory;
        private readonly ISkillTreeService _skills;
        private readonly IJournalService _journal;
        private readonly IReplicatedPlayerData<InventorySaveData> _inventoryData;
        private readonly IReplicatedPlayerData<SkillSaveData> _skillData;
        private readonly IReplicatedPlayerData<JournalSaveData> _journalData;
        private readonly IEventBus _bus;
        private readonly IGameClock _clock;
        private readonly SessionConfig _session;

        private readonly Dictionary<PersistentId, Parts> _dirty = new();
        private readonly List<KeyValuePair<PersistentId, Parts>> _sending = new();
        private float _nextSendTime;

        public PlayerProgressReplicationHost(
            IInventoryService inventory,
            ISkillTreeService skills,
            IJournalService journal,
            IReplicatedPlayerData<InventorySaveData> inventoryData,
            IReplicatedPlayerData<SkillSaveData> skillData,
            IReplicatedPlayerData<JournalSaveData> journalData,
            IEventBus bus,
            IGameClock clock,
            SessionConfig session)
        {
            _inventory = inventory;
            _skills = skills;
            _journal = journal;
            _inventoryData = inventoryData;
            _skillData = skillData;
            _journalData = journalData;
            _bus = bus;
            _clock = clock;
            _session = session;
        }

        public void Start()
        {
            if (_session.IsSinglePlayer) return;

            _inventory.OnInventoryChanged += HandleInventoryChanged;
            _skills.OnPointsChanged += HandlePointsChanged;
            _skills.OnSkillUnlocked += HandleSkillChanged;
            _skills.OnSkillLocked += HandleSkillChanged;
            _journal.OnJournalChanged += HandleJournalChanged;
        }

        public void Dispose()
        {
            _inventory.OnInventoryChanged -= HandleInventoryChanged;
            _skills.OnPointsChanged -= HandlePointsChanged;
            _skills.OnSkillUnlocked -= HandleSkillChanged;
            _skills.OnSkillLocked -= HandleSkillChanged;
            _journal.OnJournalChanged -= HandleJournalChanged;
        }

        public void LateTick()
        {
            if (_dirty.Count == 0 || _clock.Now < _nextSendTime) return;
            _nextSendTime = _clock.Now + SendInterval;

            _sending.Clear();
            _sending.AddRange(_dirty);
            _dirty.Clear();

            foreach (var (player, parts) in _sending)
            {
                _bus.PublishReplicated(new PlayerProgressEvent(
                    player,
                    (parts & Parts.Inventory) != 0 ? _inventoryData.CapturePlayer(player) : null,
                    (parts & Parts.Skills) != 0 ? _skillData.CapturePlayer(player) : null,
                    (parts & Parts.Journal) != 0 ? _journalData.CapturePlayer(player) : null));
            }
        }

        private void HandleInventoryChanged(PersistentId player) => MarkDirty(player, Parts.Inventory);
        private void HandlePointsChanged(PersistentId player, int[] _) => MarkDirty(player, Parts.Skills);
        private void HandleSkillChanged(PersistentId player, AssetId _) => MarkDirty(player, Parts.Skills);
        private void HandleJournalChanged(PersistentId player) => MarkDirty(player, Parts.Journal);

        private void MarkDirty(PersistentId player, Parts parts)
        {
            _dirty.TryGetValue(player, out Parts current);
            _dirty[player] = current | parts;
        }
    }

    /// <summary>Client only: applies <see cref="PlayerProgressEvent"/> to the local copies of the per-player services.</summary>
    public sealed class PlayerProgressReplicationClient : IStartable, IDisposable
    {
        private readonly IEventBus _bus;
        private readonly IReplicatedPlayerData<InventorySaveData> _inventory;
        private readonly IReplicatedPlayerData<SkillSaveData> _skills;
        private readonly IReplicatedPlayerData<JournalSaveData> _journal;

        public PlayerProgressReplicationClient(
            IEventBus bus,
            IReplicatedPlayerData<InventorySaveData> inventory,
            IReplicatedPlayerData<SkillSaveData> skills,
            IReplicatedPlayerData<JournalSaveData> journal)
        {
            _bus = bus;
            _inventory = inventory;
            _skills = skills;
            _journal = journal;
        }

        public void Start() => _bus.Subscribe<PlayerProgressEvent>(HandleProgress);
        public void Dispose() => _bus.Unsubscribe<PlayerProgressEvent>(HandleProgress);

        private void HandleProgress(PlayerProgressEvent e)
        {
            // The id inside the data must match the event's player; never let one restore write another player's slot.
            if (e.Inventory != null && e.Inventory.PersistentId == e.Player) _inventory.RestorePlayer(e.Inventory);
            if (e.Skills != null && e.Skills.PersistentId == e.Player) _skills.RestorePlayer(e.Skills);
            if (e.Journal != null && e.Journal.PersistentId == e.Player) _journal.RestorePlayer(e.Journal);
        }
    }
}
