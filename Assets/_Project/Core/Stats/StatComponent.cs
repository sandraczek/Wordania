using System.Collections.Generic;
using UnityEngine;

namespace Wordania.Core.Stats
{

    public class StatComponent : MonoBehaviour
    {
        public Dictionary<StatType, CharacterStat> Stats = new();
    }
}