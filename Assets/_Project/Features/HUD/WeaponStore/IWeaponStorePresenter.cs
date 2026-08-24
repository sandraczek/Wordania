using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Core.Data;
using Wordania.Core.Identifiers;
using Wordania.Features.Combat.Data;
using Wordania.Features.WeaponStore;

namespace Wordania.Features.HUD.WeaponStore
{
    public interface IWeaponStorePresenter
    {
        UniTask InitializeAsync(CancellationToken cancellation);
    }
}