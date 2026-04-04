

using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.Core.Combat;
using Wordania.Features.Combat.Data;

namespace Wordania.Features.Combat.Core
{
    public interface IWeaponFactory
    {
        WeaponController GetWeapon(WeaponData data);
        void ReturnWeapon(WeaponController controller);
        UniTask PrewarmPoolAsync(WeaponData data);
    }
}