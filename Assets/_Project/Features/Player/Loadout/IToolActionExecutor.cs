using UnityEngine;

namespace Wordania.Features.Player.Loadout
{
    public interface IToolActionExecutor
    {
        public bool ExecutePrimaryAction(Vector2 targetWorldPos, int instigatorId);
        public bool ExecuteSecondaryAction(Vector2 targetWorldPos, int instigatorId);
        void ReleasePrimaryAction();
        void ExecuteCycle();
        public void OnEquip();
        public void OnUnequip();
    }
}