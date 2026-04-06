using UnityEngine;
using Wordania.Core.Data;

namespace Wordania.Features.Bosses.Data
{
    [CreateAssetMenu(fileName = "BossRegistry", menuName = "Bosses/Database")]
    public sealed class BossRegistry: AssetRegistry<BossTemplate>
    {
        
    }
}