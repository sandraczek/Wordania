using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wordania.Identifiers
{
    public class Entity : MonoBehaviour
    {
        public InstanceId InstanceId;
        private readonly Dictionary<Type, object> _features = new();
        public Transform Transform => transform;

        public bool TryGetFeature<T>(out T feature) where T : class
        {
            if (!_features.TryGetValue(typeof(T), out var obj))
            {
                TryGetComponent(out T component);
                obj = component;
                _features[typeof(T)] = obj;
            }

            feature = obj as T;
            return feature != null;
        }
    }
}