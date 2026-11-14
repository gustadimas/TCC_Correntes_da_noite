#if UNITY_EDITOR
using UnityEngine;

namespace CorrentesDaNoite
{
    public class DebugPhaseStart : MonoBehaviour
    {
        [SerializeField] PhaseStart.Phase _phase;
        [SerializeField] PhaseStart _phaseStart;

        private void OnEnable() => _phaseStart.StartAt(_phase);
    }
}
#endif