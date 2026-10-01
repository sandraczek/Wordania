

using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.Combat;
using Wordania.Combat.Data;

namespace Wordania.Combat.Core
{
    public interface IWeaponFactory
    {
        WeaponController GetWeapon(WeaponData data);
        void ReturnWeapon(WeaponController controller);
        UniTask PrewarmPoolAsync(WeaponData data);
    }
}