using UnityEngine;
using CorrentesDaNoite.Audio;

namespace CorrentesDaNoite.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerSoundEmitter : MonoBehaviour
    {
        [Header("Sound Ranges")]
        [SerializeField] protected float walkSoundRange = 5f;
        [SerializeField] protected float runSoundRange = 15f;
        [SerializeField] protected float jumpSoundRange = 10f;
        [SerializeField] protected float landSoundRange = 12f;

        [Header("Sound Cooldowns")]
        [SerializeField] protected float walkSoundCooldown = 0.5f;
        [SerializeField] protected float runSoundCooldown = 0.3f;
        [SerializeField] protected float movementSoundMinSpeed = 0.5f;

        [Header("Debug")]
        [SerializeField] protected bool enableDebugGizmos = false;
        [SerializeField] protected bool showDebugLogs = false;

        protected PlayerController playerController;
        protected CharacterController characterController;

        protected float lastWalkSoundTime;
        protected float lastRunSoundTime;
        protected bool wasGroundedLastFrame;
        protected Vector3 lastSoundPosition;
        protected SoundType lastSoundType;

        protected virtual void Awake()
        {
            playerController = GetComponent<PlayerController>();
            characterController = GetComponent<CharacterController>();
        }

        protected virtual void Start()
        {
            wasGroundedLastFrame = characterController.isGrounded;
        }

        protected virtual void Update()
        {
            if (playerController.IsCaptured) return;

            CheckMovementSounds();
            CheckLandingSound();
        }

        protected virtual void CheckMovementSounds()
        {
            if (characterController.isGrounded && playerController.CurrentSpeed >= movementSoundMinSpeed)
            {
                if (playerController.IsCrouching)
                {
                    return;
                }

                if (playerController.IsRunning)
                {
                    if (Time.time >= lastRunSoundTime + runSoundCooldown)
                    {
                        EmitRunSound();
                        lastRunSoundTime = Time.time;
                    }
                }
                else
                {
                    if (Time.time >= lastWalkSoundTime + walkSoundCooldown)
                    {
                        EmitWalkSound();
                        lastWalkSoundTime = Time.time;
                    }
                }
            }
        }

        protected virtual void CheckLandingSound()
        {
            bool isGroundedNow = characterController.isGrounded;

            if (!wasGroundedLastFrame && isGroundedNow)
                EmitLandSound();

            wasGroundedLastFrame = isGroundedNow;
        }

        public virtual void EmitWalkSound() => EmitSound(SoundType.Walking, walkSoundRange);

        public virtual void EmitRunSound() => EmitSound(SoundType.Running, runSoundRange);

        public virtual void EmitJumpSound() => EmitSound(SoundType.Jumping, jumpSoundRange);

        public virtual void EmitLandSound() => EmitSound(SoundType.Landing, landSoundRange);

        protected virtual void EmitSound(SoundType soundType, float range)
        {
            lastSoundPosition = transform.position;
            lastSoundType = soundType;

            SoundEmitter.EmitSound(transform.position, range, soundType, gameObject);

            if (showDebugLogs)
                Debug.Log($"[PlayerSoundEmitter] Emitiu {soundType} com raio {range}m");
        }

        protected virtual void OnDrawGizmosSelected()
        {
            if (!enableDebugGizmos) return;

            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, walkSoundRange);

            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, runSoundRange);

            Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
            Gizmos.DrawSphere(transform.position, jumpSoundRange);

            if (Application.isPlaying && lastSoundPosition != Vector3.zero)
            {
                Gizmos.color = GetSoundColor(lastSoundType);
                Gizmos.DrawWireSphere(lastSoundPosition, GetSoundRange(lastSoundType));
            }
        }

        protected virtual Color GetSoundColor(SoundType soundType)
        {
            return soundType switch
            {
                SoundType.Walking => Color.yellow,
                SoundType.Running => Color.red,
                SoundType.Jumping => Color.cyan,
                SoundType.Landing => Color.magenta,
                _ => Color.white
            };
        }

        protected virtual float GetSoundRange(SoundType soundType)
        {
            return soundType switch
            {
                SoundType.Walking => walkSoundRange,
                SoundType.Running => runSoundRange,
                SoundType.Jumping => jumpSoundRange,
                SoundType.Landing => landSoundRange,
                _ => 0f
            };
        }
    }
}