using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    [CreateAssetMenu(menuName = "CorrentesDaNoite/Enemies/WatchGuard Config", fileName = "WatchGuardConfig")]
    public class WatchGuardConfig : ScriptableObject
    {
        [Header("Watch Guard Settings")]
        public float guardMoveSpeed = 2f;
        public float guardArrivalThreshold = 0.4f;
        public float guardLookRotationSpeed = 180f;
        public float guardLookDuration = 2f;
        public float guardLookAlignmentTolerance = 2f;
        public bool guardLoopPoints = true;
    }
}
