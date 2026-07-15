using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Wordania.Core.Combat;
using Wordania.Core.Gameplay;
using Wordania.Core.Identifiers;

namespace Wordania.Features.Mechanics
{
    [RequireComponent(typeof(IEntityContext))]
    public class EntityMechanicController : MonoBehaviour
    {
        private IMechanicFactory _factory;

        private IEntityContext _context;
        private readonly Dictionary<AssetId, IMechanic> _activeMechanics = new();


        [Inject]
        public void Construct(IMechanicFactory mechanicFactory)
        {
            _factory = mechanicFactory;
        }
        private void Awake()
        {
            _context = GetComponent<IEntityContext>();
        }

        public void EnableMechanic(AssetId mechanicId)
        {
            if (_activeMechanics.ContainsKey(mechanicId)) return;

            IMechanic mechanic = _factory.CreateMechanic(mechanicId);

            if (!mechanic.OnActivate(_context)) return;

            _activeMechanics.Add(mechanicId, mechanic);
        }

        public void DisableMechanic(AssetId mechanicId)
        {
            if (_activeMechanics.TryGetValue(mechanicId, out var mechanic))
            {
                mechanic.OnDeactivate();
                _activeMechanics.Remove(mechanicId);
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            foreach (var mechanic in _activeMechanics.Values)
            {
                mechanic.OnTick(dt);
            }
        }

        public bool HasMechanic(AssetId mechanicId)
        {
            return _activeMechanics.ContainsKey(mechanicId);
        }

        public void ClearAllMechanics()
        {
            var mechanicsToRemove = _activeMechanics.Keys;

            foreach (var mechanicId in mechanicsToRemove)
            {
                DisableMechanic(mechanicId);
            }
        }
    }
}