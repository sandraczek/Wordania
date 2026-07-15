// using System.Collections.Generic;
// using UnityEngine;
// using Wordania.Core.Attributes;
// using Wordania.Core.Constants;
// using Wordania.Core.Data;
// using Wordania.Core.Identifiers;
// using Wordania.Features.Skills.Effects;

// namespace Wordania.Features.Journal.Milestone
// {
//     [CreateAssetMenu(fileName = "NewMilestone", menuName = "Journal/Milestone")]
//     public class JournalMilestone : DataAsset
//     {
//         [SerializeReference] public AssetId TargetId;
//         public int TargetThreshold;

//         [SerializeReference, SubclassSelector] private List<MechanicIds> _effects = new();

//         public IReadOnlyList<ISkillEffect> Effects => _effects;
//     }
// }