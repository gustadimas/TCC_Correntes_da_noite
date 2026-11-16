using UnityEngine;
using CorrentesDaNoite.Audio;
using CorrentesDaNoite.Audio.Footsteps;

namespace CorrentesDaNoite.Enemies
{
    [RequireComponent(typeof(EnemyMovement))]
    public class EnemyFootstepEmitter : MonoBehaviour
    {
        const float CAPTION_DISTANCE = 8f;

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
        EnemyController _controller;

        void Awake()
        {
            movement = GetComponent<EnemyMovement>();
            _controller = GetComponent<EnemyController>();
            // o atalho de null do c# nao pega o null falso da unity no campo vazio
            if (surfaceTracker == null) surfaceTracker = GetComponent<FootstepSurfaceTracker>();
            if (feetTransform == null) feetTransform = transform;
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
            if (IsNearPlayer()) UI.VoiceSubtitles.Instance?.Caption("[passos por perto]");
            if (_controller != null) UI.EnemyMarkers.Instance?.Step(_controller, feetTransform);
        }

        bool IsNearPlayer() => _controller != null && _controller.PlayerTransform != null && (_controller.PlayerTransform.position - transform.position).sqrMagnitude < CAPTION_DISTANCE * CAPTION_DISTANCE;
    }
}
