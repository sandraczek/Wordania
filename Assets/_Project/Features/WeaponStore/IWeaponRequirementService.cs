
using System;
using VContainer.Unity;
using Wordania.Combat.Events;
using Wordania.Events;
using Wordania.Identifiers;

namespace Wordania.WeaponStore
{
    public interface IWeaponRequirementService
    {
        bool CheckRequirements(AssetId id);
    }
}