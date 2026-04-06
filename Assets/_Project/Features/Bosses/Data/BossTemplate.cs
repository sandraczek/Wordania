using System.Collections.Generic;
using UnityEngine;
using Wordania.Core.Data;
using Wordania.Core.Identifiers;
using Wordania.Features.Bosses.Core;

namespace Wordania.Features.Bosses.Data
{
    public abstract class BossTemplate : DataAsset
    {
        [field: SerializeField] public string DisplayName {get; private set; }
        [field: SerializeField] public BossController Prefab { get; private set; }

#if UNITY_EDITOR
        override protected void OnValidate()
        {
            base.OnValidate();
            if (Prefab != null && Prefab.GetComponent<BossController>() == null)
            {
                Debug.LogError($"[BossTemplate] The assigned prefab '{Prefab.name}' does not contain a BossController script! Rejected.");
                Prefab = null;
            }
        }
#endif
    }
}