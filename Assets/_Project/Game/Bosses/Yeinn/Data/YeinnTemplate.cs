using UnityEngine;
using Wordania.Bosses.Data;

namespace Wordania.Bosses.Yeinn.Data
{
    [CreateAssetMenu(fileName = "YeinnTemplate", menuName = "Bosses/Yeinn/Template")]
    public sealed class YeinnTemplate: BossTemplate
    {
        [field: Header("Anatomy Data")]
        [field: SerializeField] public YeinnHeadData Head { get; private set; }
        [field: SerializeField] public YeinnHandData LeftHand { get; private set; }
        [field: SerializeField] public YeinnHandData RightHand { get; private set; }

        [field: Header("Phase Data")]
        [field: SerializeField] public YeinnPhaseOneData PhaseOneData { get; private set; }
        [field: SerializeField] public YeinnPhaseTwoData PhaseTwoData { get; private set; }
    }
}