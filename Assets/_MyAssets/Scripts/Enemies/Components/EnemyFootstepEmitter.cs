using UnityEngine;
using CorrentesDaNoite.Audio;
using CorrentesDaNoite.Audio.Footsteps;

namespace CorrentesDaNoite.Enemies
{
    [RequireComponent(typeof(EnemyMovement))]
    public class EnemyFootstepEmitter : MonoBehaviour
    {
        [Header("Footstep Timing")]
        [SerializeField] internal float movementThreshold = 0.2f;
        [SerializeField] internal float stepInterval = 0.4f;
        [SerializeField] internal bool useAnimationEvents;

        [Header("Surface Configs")]
        [SerializeField] internal FootstepConfig defaultConfig;
        [SerializeField] internal FootstepConfig surfaceConfig;
        [SerializeField] internal FootstepSurfaceTracker surfaceTracker;
        [SerializeField] internal Transform feetTransform;
        [SerializeField] internal bool usePositionalAudio = true;

        EnemyMovement movement;
        float lastStepTime;

        void Awake()
        {
            movement = GetComponent<EnemyMovement>();
            surfaceTracker ??= GetComponent<FootstepSurfaceTracker>();
            feetTransform ??= transform;
        }

        void Update()
        {
            if (useAnimationEvents)
                return;

            if (movement == null || movement.IsMoving == false)
                return;

            if (Time.time - lastStepTime < stepInterval)
                return;

            lastStepTime = Time.time;
            PlayFootstep();
        }

        public void PlayFootstep()
        {
            FootstepConfig config = surfaceConfig;
            if (config.clips == null || config.clips.Length == 0)
                config = defaultConfig;

            if (config.clips == null || config.clips.Length == 0)
                return;

            AudioConfig audioConfig = config.GetRandomAudioConfig();
            Vector3? pos = usePositionalAudio && feetTransform != null ? feetTransform.position : (Vector3?)null;
            AudioManager.Instance?.PlayConfig(audioConfig, pos);
        }
    }
}
