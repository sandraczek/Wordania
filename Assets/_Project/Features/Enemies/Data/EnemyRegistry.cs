using UnityEngine;
using Wordania.Data;

namespace Wordania.Enemies.Data
{
    [CreateAssetMenu(fileName = "EnemyRegistry", menuName = "Enemies/Registry")]
    public sealed class EnemyRegistry : AssetRegistry<EnemyTemplate>
    {

    }
}