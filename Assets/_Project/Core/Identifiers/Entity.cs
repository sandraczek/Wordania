using System;
using System.Collections.Generic;
using UnityEngine;
using Wordania.Core.Combat;
using Wordania.Core.Gameplay;
using Wordania.Core.Identifiers;
using Wordania.Core.Mechanics;
using Wordania.Features.Identifiers;

namespace Wordania.Core.Identifiers
{
    public class Entity : MonoBehaviour
    {
        public InstanceId InstanceId;
        private readonly Dictionary<Type, object> _features = new();
        public Transform Transform => transform;

        private void Awake()
        {
            TryRegister<IDamageable>();
            TryRegister<ITrackable>();
            TryRegister<IEnemy>();
            TryRegister<IPersistent>();
            TryRegister<IReadOnlyHealth>();
        }

        private void RegisterFeature<T>(T feature) where T : class
        {
            if (feature != null)
            {
                _features[typeof(T)] = feature;
            }
        }

        private void TryRegister<T>() where T : class
        {
            if (TryGetComponent(out T component))
            {
                RegisterFeature(component);
            }
        }

        public bool TryGetFeature<T>(out T feature) where T : class
        {
            if (_features.TryGetValue(typeof(T), out var obj))
            {
                feature = (T)obj;
                return true;
            }
#if UNITY_EDITOR
            else if (TryGetComponent(out T c))
            {
                feature = c;
                RegisterFeature(c);
                return true;
            }
#endif
            feature = null;
            return false;
        }
    }
}