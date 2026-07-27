namespace Wordania.Features.Mechanics
{
    using System.Collections.Generic;
    using UnityEngine;
    using VContainer;
    using Wordania.Core.Combat;
    using Wordania.Core.Gameplay;
    using Wordania.Core.Identifiers;
    using Wordania.Core.Mechanics;

    [RequireComponent(typeof(IEntityContext))]
    public class EntityMechanicController : MonoBehaviour, IEntityMechanicController
    {
        private IMechanicFactory _factory;
        private IEntityContext _context;

        private readonly Dictionary<AssetId, MechanicTracker> _activeMechanics = new(8);

        private readonly List<ITickableMechanic> _tickableMechanics = new(4);

        private readonly Stack<MechanicTracker> _trackerPool = new(8);

        [Inject]
        public void Construct(IMechanicFactory mechanicFactory)
        {
            _factory = mechanicFactory;
        }

        private void Awake()
        {
            _context = GetComponent<IEntityContext>();
        }

        public void EnableMechanic(AssetId mechanicId, InstanceId source)
        {
            if (!_activeMechanics.TryGetValue(mechanicId, out MechanicTracker tracker))
            {
                tracker = GetTrackerFromPool();

                IMechanic mechanic = _factory.CreateMechanic(mechanicId);

                if (!mechanic.OnActivate(_context))
                {
                    mechanic.OnDeactivate();
                    _factory.ReleaseMechanic(mechanicId, mechanic);

                    ReturnTrackerToPool(tracker);
                    return;
                }

                tracker.Mechanic = mechanic;
                _activeMechanics.Add(mechanicId, tracker);

                if (mechanic is ITickableMechanic tickable)
                {
                    _tickableMechanics.Add(tickable);
                }
            }

            if (!tracker.Sources.Contains(source))
            {
                tracker.Sources.Add(source);
            }
        }

        public void DisableMechanic(AssetId mechanicId, InstanceId source)
        {
            if (!_activeMechanics.TryGetValue(mechanicId, out MechanicTracker tracker))
            {
                return;
            }

            if (!tracker.Sources.Remove(source))
            {
                return;
            }

            if (tracker.Sources.Count == 0)
            {
                tracker.Mechanic.OnDeactivate();
                _factory.ReleaseMechanic(mechanicId, tracker.Mechanic);

                if (tracker.Mechanic is ITickableMechanic tickable)
                {
                    _tickableMechanics.Remove(tickable);
                }
                _activeMechanics.Remove(mechanicId);

                ReturnTrackerToPool(tracker);
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            for (int i = 0; i < _tickableMechanics.Count; i++)
            {
                _tickableMechanics[i].OnTick(dt);
            }
        }

        public bool HasMechanic(AssetId mechanicId)
        {
            return _activeMechanics.ContainsKey(mechanicId);
        }

        public void ClearAllMechanics()
        {
            foreach (var pair in _activeMechanics)
            {
                MechanicTracker tracker = pair.Value;
                tracker.Mechanic.OnDeactivate();
                _factory.ReleaseMechanic(pair.Key, tracker.Mechanic);

                ReturnTrackerToPool(tracker);
            }

            _activeMechanics.Clear();
            _tickableMechanics.Clear();
        }

        private MechanicTracker GetTrackerFromPool()
        {
            if (_trackerPool.TryPop(out MechanicTracker tracker))
            {
                return tracker;
            }

            return new MechanicTracker();
        }

        private void ReturnTrackerToPool(MechanicTracker tracker)
        {
            tracker.Mechanic = null;
            tracker.Sources.Clear();
            _trackerPool.Push(tracker);
        }

        private class MechanicTracker
        {
            public IMechanic Mechanic;

            public readonly List<InstanceId> Sources = new(1);
        }
    }
}