using System;
using System.Collections.Generic;
using UnityEngine;
using Wordania.Core.Identifiers;
using Wordania.Core.Mechanics;

namespace Wordania.Core.Identifiers
{
    public class Entity : MonoBehaviour
    {
        public InstanceId InstanceId;
        private readonly Dictionary<Type, object> _features = new();

        private void Awake()
        {

        }

        private void RegisterFeature<T>(T feature) where T : class
        {
            if (feature != null)
            {
                _features[typeof(T)] = feature;
            }
        }

        public bool TryGetFeature<T>(out T feature) where T : class
        {
            if (_features.TryGetValue(typeof(T), out var obj))
            {
                feature = (T)obj;
                return true;
            }
            feature = null;
            return false;
        }
    }
}