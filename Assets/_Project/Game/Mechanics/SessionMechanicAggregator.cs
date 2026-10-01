
// using System.Collections.Generic;
// using Wordania.Identifiers;
// using Wordania.Journal.Milestones;
// using Wordania.Skills;

// namespace Wordania.Mechanics
// {
//     public class SessionMechanicsAggregator
//     {
//         private readonly ISkillTreeService _skills;
//         private readonly IJournalMilestoneService _milestones;
//         private readonly 

//         public IEnumerable<(AssetId mechanicId, InstanceId sourceId)> GetPersistentMechanics(string persistentId)
//         {
//             var result = new List<(AssetId, InstanceId)>();

//             foreach (var mech in _milestones.GetEarnedMechanics(persistentId))
//                 result.Add((mech, InstanceId.Journal));

//             foreach (var mech in _skills.GetUnlockedMechanics(persistentId))
//                 result.Add((mech, InstanceId.SkillTree));

//             return result;
//         }
//     }
// }