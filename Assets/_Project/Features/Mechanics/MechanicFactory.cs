using System;
using System.Collections.Generic;
using VContainer;
using Wordania.Core.Data;
using Wordania.Core.Identifiers;
using Wordania.Features.Mechanics.Data;

namespace Wordania.Features.Mechanics
{
    public sealed class MechanicFactory : IMechanicFactory
    {
        private readonly IObjectResolver _resolver;
        private readonly IAssetRegistry<MechanicData> _registry;
        private readonly Dictionary<AssetId, Stack<IMechanic>> _pools = new();

        public MechanicFactory(IObjectResolver resolver, IAssetRegistry<MechanicData> registry)
        {
            _resolver = resolver;
            _registry = registry;
        }

        public IMechanic CreateMechanic(AssetId mechanicId)
        {
            if (_pools.TryGetValue(mechanicId, out var stack) && stack.Count > 0)
            {
                return stack.Pop();
            }

            MechanicData definition = _registry.Get(mechanicId);
            return definition.CreateRuntimeInstance(_resolver);
        }

        public void ReleaseMechanic(AssetId mechanicId, IMechanic mechanic)
        {
            if (mechanic == null) return;

            if (!_pools.TryGetValue(mechanicId, out var stack))
            {
                stack = new Stack<IMechanic>();
                _pools[mechanicId] = stack;
            }

            stack.Push(mechanic);
        }
    }
}