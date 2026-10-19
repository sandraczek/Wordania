using System;
using UnityEngine;
using VContainer;
using Wordania.Identifiers;
using Wordania.Services;
using Wordania.Mechanics;
using Wordania.Mechanics.Data;

namespace Wordania.Player
{
    [RequireComponent(typeof(MechanicsComponent))]
    public class PlayerDebugHandler : MonoBehaviour
    {
        private IDebugService _debugService;
        private MechanicIds _mechanicIds;
        private MechanicsComponent _mechanics;
        private Player _player;

        [Inject]
        public void Construct(IDebugService debugService, MechanicIds mechanicIds)
        {
            _debugService = debugService;
            _mechanicIds = mechanicIds;
        }
        public void Awake()
        {
            _mechanics = GetComponent<MechanicsComponent>();
            _player = GetComponent<Player>();
        }

        private void OnEnable()
        {
            _debugService.OnGodModeChanged += HandleGodModeChanged;

            Refresh();
        }

        /// <summary>Re-applies the debug state. Call after the player's local control changes.</summary>
        public void Refresh()
        {
            if (_debugService != null) HandleGodModeChanged(_debugService.IsGodModeActive);
        }

        private void OnDisable()
        {
            if (_debugService != null)
            {
                _debugService.OnGodModeChanged -= HandleGodModeChanged;
            }
        }

        private void HandleGodModeChanged(bool isGodMode)
        {
            // Debug toggle belongs to this machine's own player. On the host every remote player has this component
            // too and would otherwise become invincible as well.
            if (!_player.IsLocal) return;

            if (isGodMode) _mechanics.EnableMechanic(_mechanicIds.GodMode, InstanceId.Debug);
            else _mechanics.DisableMechanic(_mechanicIds.GodMode, InstanceId.Debug);
        }
    }
}