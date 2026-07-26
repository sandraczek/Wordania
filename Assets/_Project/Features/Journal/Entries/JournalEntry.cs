using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wordania.Core.Data;
using Wordania.Core.Identifiers;
using Wordania.Features.Journal.Milestones;


namespace Wordania.Features.Journal.Entries
{
    public abstract class JournalEntry : DataAsset
    {
        public List<JournalMilestone> Milestones;
        public abstract AssetId TargetId { get; }
        public abstract JournalCategory Category { get; }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (Milestones is { Count: > 1 })
                Milestones.Sort((a, b) => a.TargetThreshold.CompareTo(b.TargetThreshold));
        }
#endif
    }

    public abstract class JournalEntry<TSource> : JournalEntry where TSource : DataAsset
    {
        [SerializeField] private TSource _source;
        public TSource Source => _source;
        public sealed override AssetId TargetId => _source != null ? _source.Id : AssetId.Empty;
    }

    public enum JournalCategory
    {
        Enemies,
        Bosses,
        Blocks
    }
}