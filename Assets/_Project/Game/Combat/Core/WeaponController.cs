using UnityEngine;
using Wordania.Combat.Data;

namespace Wordania.Combat.Core
{
    /// <summary>Weapon view held in the player's hand. Firing is resolved by ICombatAuthority, not here.</summary>
    public class WeaponController : MonoBehaviour
    {
        [HideInInspector] public WeaponData Data;

        public void Initialize(WeaponData data)
        {
            Data = data;
        }


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position + new Vector3(-2f, -1f, 0f), new(1f, 1f, 0f));
            Gizmos.DrawWireSphere(transform.position, 0.1f);
        }
#endif
    }
}