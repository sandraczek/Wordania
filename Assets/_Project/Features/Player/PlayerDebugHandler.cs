using System;
using UnityEngine;
using VContainer;
using Wordania.Core.Combat;
using Wordania.Core.Identifiers;
using Wordania.Core.Services;
using Wordania.Features.Combat;
using Wordania.Features.Mechanics;
using Wordania.Features.Mechanics.Data;

namespace Wordania.Features.Player
{
    [RequireComponent(typeof(MechanicsComponent))]
    public class PlayerDebugHandler : MonoBehaviour
    {
        private IDebugService _debugService;
        private MechanicIds _mechanicIds;
        private MechanicsComponent _mechanics;

        [Inject]
        public void Construct(IDebugService debugService, MechanicIds mechanicIds)
        {
            _debugService = debugService;
            _mechanicIds = mechanicIds;
        }
        public void Awake()
        {
            _mechanics = GetComponent<MechanicsComponent>();
        }

        private void OnEnable()
        {
            _debugService.OnGodModeChanged += HandleGodModeChanged;

            HandleGodModeChanged(_debugService.IsGodModeActive);
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
            if (isGodMode) _mechanics.EnableMechanic(_mechanicIds.GodMode, InstanceId.Debug);
            else _mechanics.DisableMechanic(_mechanicIds.GodMode, InstanceId.Debug);
        }
    }
}