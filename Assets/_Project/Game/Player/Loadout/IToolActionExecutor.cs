using UnityEngine;
using Wordania.Identifiers;

namespace Wordania.Player.Loadout
{
    public interface IToolActionExecutor
    {
        public bool ExecutePrimaryAction(Vector2 targetWorldPos, InstanceId instigatorId);
        public bool ExecuteSecondaryAction(Vector2 targetWorldPos, InstanceId instigatorId);
        void ReleasePrimaryAction();
        void ExecuteCycle();
        public void OnEquip();
        public void OnUnequip();
    }
}