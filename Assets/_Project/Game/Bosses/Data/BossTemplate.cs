using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wordania.Data;
using Wordania.Bosses.Core;
using Wordania.Skills;

namespace Wordania.Bosses.Data
{
    public abstract class BossTemplate : DataAsset
    {
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public BossController Prefab { get; private set; }
        [field: SerializeField] public RewardData Reward { get; private set; }

#if UNITY_EDITOR
        override protected void OnValidate()
        {
            base.OnValidate();
            if (Prefab != null && Prefab.GetComponent<BossController>() == null)
            {
                Debug.LogError($"[BossTemplate] The assigned prefab '{Prefab.name}' does not contain a BossController script! Rejected.");
                Prefab = null;
            }

            Reward?.EditorSortThreshold();
        }
#endif
    }
}