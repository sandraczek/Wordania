using System;
using System.Collections.Generic;
using UnityEngine;
using Wordania.Identifiers;
using Wordania.Mechanics.Data;

namespace Wordania.Journal.Milestones
{

    [Serializable]
    public struct JournalMilestone
    {
        [Min(0)] public int TargetThreshold;

        public MechanicData Mechanic;

    }
}