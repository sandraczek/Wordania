using UnityEngine;
using VContainer;
using Wordania.Data;
using Wordania.Mechanics;

namespace Wordania.Mechanics.Data
{
    public abstract class MechanicData : DataAsset
    {
        public abstract IMechanic CreateRuntimeInstance(IObjectResolver resolver);
    }
}