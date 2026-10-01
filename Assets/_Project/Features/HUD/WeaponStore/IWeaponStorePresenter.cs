using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Data;
using Wordania.Identifiers;
using Wordania.Combat.Data;
using Wordania.WeaponStore;

namespace Wordania.HUD.WeaponStore
{
    public interface IWeaponStorePresenter
    {
        UniTask InitializeAsync(CancellationToken cancellation);
    }
}