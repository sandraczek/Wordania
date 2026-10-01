using UnityEngine;
using Wordania.Data;

namespace Wordania.Bosses.Data
{
    [CreateAssetMenu(fileName = "BossRegistry", menuName = "Bosses/Database")]
    public sealed class BossRegistry: AssetRegistry<BossTemplate>
    {
        
    }
}