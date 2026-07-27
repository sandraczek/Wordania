using System;
using System.Collections.Generic;
using UnityEngine;
using Wordania.Core.Attributes;
using Wordania.Core.Data;
using Wordania.Core.Identifiers;
using Wordania.Features.Mechanics.Data;

namespace Wordania.Features.Journal.Milestones
{

    [Serializable]
    public struct JournalMilestone
    {
        [Min(0)] public int TargetThreshold;

        public MechanicData Mechanic;

    }
}