using UnityEngine;
using VContainer;
using Wordania.Core.Data;
using Wordania.Core.Identifiers;
using Wordania.Features.Mechanics;

namespace Wordania.Features.Mechanics.Data
{
    public abstract class MechanicData : DataAsset
    {
        public abstract IMechanic CreateRuntimeInstance(IObjectResolver resolver);
    }
}