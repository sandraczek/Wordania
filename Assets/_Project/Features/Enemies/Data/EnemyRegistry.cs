using UnityEngine;
using Wordania.Core.Data;
using Wordania.Core.Identifiers;

namespace Wordania.Features.Enemies.Data
{
    [CreateAssetMenu(fileName = "EnemyRegistry", menuName = "Enemies/Registry")]
    public sealed class EnemyRegistry : AssetRegistry<EnemyTemplate>
    {

    }
}