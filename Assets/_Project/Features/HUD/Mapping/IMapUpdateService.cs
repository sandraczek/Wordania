using System;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Config;
using Wordania.World;

namespace Wordania.Mapping
{
    public interface IMapUpdateService
    {
        public UniTask RenderInitialMapAsync(CancellationToken token);
    }
}