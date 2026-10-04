using System;
using System.Collections.Generic;
using VContainer.Unity;
using Wordania.Combat.Events;
using Wordania.Constants;
using Wordania.Data;
using Wordania.Events;
using Wordania.Gameplay;
using Wordania.Identifiers;
using Wordania.Bosses.Data;
using Wordania.Enemies.Data;
using Wordania.Journal.Entries;
using Wordania.World.Events;

namespace Wordania.Journal.Milestones
{
    public interface IJournalMilestoneService
    {
        void ApplyEarnedMilestones(PersistentId persistentId, IReadOnlyDictionary<AssetId, int> enemyKills);
    }
}