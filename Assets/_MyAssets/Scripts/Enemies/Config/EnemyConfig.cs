using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    [CreateAssetMenu(menuName = "CorrentesDaNoite/Enemies/EnemyConfig", fileName = "EnemyConfig")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("Movement")]
        public float patrolSpeed = 2f;
        public float chaseSpeed = 7f;
        public float turnSpeed = 5f;

        [Header("Timing")]
        public float spottedDelay = 0.5f;

        [Header("Capture")]
        public float captureDistance = 2f;

        [Header("Detection - Light")]
        public float lightRadius = 15f;

        [Header("Detection - Sound")]
        public float hearingRange = 20f;
        public float minSoundDistanceToReact = 3f;
        public int soundsToTriggerChase = 3;
        public float soundAlertDuration = 3f;
        public float soundRotationSpeed = 3f;
        public bool reactToWalkingSounds = true;
        public bool reactToRunningSounds = true;
        public bool reactToJumpingSounds = false;
        public bool enableSoundReactions = true;
    }
}
