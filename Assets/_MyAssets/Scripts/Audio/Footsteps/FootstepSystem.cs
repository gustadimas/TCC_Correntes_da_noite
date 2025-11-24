using UnityEngine;
using CorrentesDaNoite.Audio;

namespace CorrentesDaNoite.Audio.Footsteps
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(FootstepSurfaceTracker))]
    public class FootstepSystem : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] internal Transform feetTransform;
        [SerializeField] internal CharacterController characterController;
        [SerializeField] internal FootstepSurfaceTracker surfaceTracker;

        [Header("Configurações de Passos")]
        [SerializeField] internal float movementThreshold = 0.5f;
        [SerializeField] internal bool useAnimationEvents;
        [SerializeField] internal bool usePositionalAudio = true;
        [SerializeField] internal FootstepConfig defaultConfig;

        [Header("Superfícies")]
        [SerializeField] internal FootstepConfig calcadaConfig;
        [SerializeField] internal FootstepConfig metalConfig;
        [SerializeField] internal FootstepConfig normalConfig;
        [SerializeField] internal FootstepConfig tetoBarracaConfig;
        [SerializeField] internal FootstepConfig gramaConfig;
        [SerializeField] internal FootstepConfig terraConfig;
        [SerializeField] internal FootstepConfig pedraConfig;

        protected float lastStepTime;
        protected Vector3 lastPosition;

        protected void Reset()
        {
            characterController = GetComponent<CharacterController>();
            surfaceTracker = GetComponent<FootstepSurfaceTracker>();
            feetTransform = transform;
        }

        protected void Awake()
        {
            characterController ??= GetComponent<CharacterController>();
            surfaceTracker ??= GetComponent<FootstepSurfaceTracker>();
            feetTransform ??= transform;
            lastPosition = transform.position;
        }

        protected void Update()
        {
            if (useAnimationEvents)
                return;

            TryPlayStepByMovement();
        }

        public void PlayStepAnimationEvent()
        {
            TryPlayFootstep(surfaceTracker.DetectByRaycast(feetTransform));
        }

        protected void TryPlayStepByMovement()
        {
            if (characterController == null || !characterController.isGrounded)
                return;

            Vector3 delta = transform.position - lastPosition;
            float distance = new Vector2(delta.x, delta.z).magnitude;
            lastPosition = transform.position;

            if (distance < movementThreshold * Time.deltaTime)
                return;

            TryPlayFootstep(surfaceTracker.DetectByRaycast(feetTransform));
        }

        protected void TryPlayFootstep(SurfaceType surfaceType)
        {
            FootstepConfig config = ResolveConfig(surfaceType);
            if (config.clips == null || config.clips.Length == 0)
                config = defaultConfig;

            if (config.clips == null || config.clips.Length == 0)
                return;

            if (Time.time - lastStepTime < config.minTimeBetweenSteps)
                return;

            lastStepTime = Time.time;
            AudioConfig audioConfig = config.GetRandomAudioConfig();
            Vector3? position = usePositionalAudio && feetTransform != null ? feetTransform.position : (Vector3?)null;
            AudioManager.Instance?.PlayConfig(audioConfig, position);
        }

        protected FootstepConfig ResolveConfig(SurfaceType surfaceType)
        {
            return surfaceType switch
            {
                SurfaceType.Calcada => calcadaConfig,
                SurfaceType.Normal => normalConfig,
                SurfaceType.Grama => gramaConfig,
                SurfaceType.Terra => terraConfig,
                SurfaceType.Pedra => pedraConfig,
                _ => defaultConfig
            };
        }
    }
}