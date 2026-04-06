using UnityEngine;
using Wordania.Core.Events;
using Wordania.Features.Bosses.Core;

namespace Wordania.Features.Bosses.Events
{
    [CreateAssetMenu(menuName = "Signals/BossSpawned")]
    public sealed class BossSpawnedSignal: BaseSignal<BossController>
    {

    }
}