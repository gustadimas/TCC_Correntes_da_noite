using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using CorrentesDaNoite.Enemies;
using CorrentesDaNoite.Player;
using Unity.Cinemachine;

namespace CorrentesDaNoite.Chase
{
    public class ChaseEnemyController : MonoBehaviour
    {
        [Header("Chase Behavior")]
        [SerializeField] Transform target;

        [Header("Capture")]
        [SerializeField] float captureDistance = 1.4f;
        [SerializeField] string captureAnimationTrigger = "Capture";
        [SerializeField] float captureKillDelay = 1.5f;
        [SerializeField] Transform playerHoldPoint;
        [SerializeField] CaptureHandler captureHandler;

        protected float aggressiveChaseSpeed = 5.2f;
        protected float minDistance = 1f;

        // com ela correndo ele vai ficando pra tras ate essa distancia e o tropeco da corrida traz ele de volta
        const float CHASE_GAP = 10f;
        const float PLAYER_RUNNING_SPEED = 4f;
        const float NEAR_CATCH_FX_DISTANCE = 2.3f;

        [Header("Movement Settings")]
        [SerializeField] Vector3 startOffset = new Vector3(0f, 0f, -15f);
        [SerializeField] float fixedHeight = 0f;
        [SerializeField] bool useFixedHeight = true;
        [SerializeField] float rotationSpeed = 5f;
        [SerializeField] float movementSmoothness = 8f;

        [Header("State")]
        [SerializeField] bool isChasing;

        [Header("Animation")]
        [SerializeField] Animator animator;
        [SerializeField] string runAnimationBool = "IsRunning";
        [SerializeField] string speedAnimationFloat = "Speed";
        [SerializeField] string roarAnimationTrigger = "Roar";
        [SerializeField] string roarStateName = "Roar";
        [SerializeField] int roarLayerIndex = 0;
        [SerializeField] float roarCrossFade = 0.05f;

        [Header("Audio")]
        [SerializeField] AudioClip roarSound;
        [SerializeField] AudioSource audioSource;
        [SerializeField] Audio.AudioEvent roarAudioEvent;
        [SerializeField, Range(0f, 1.5f)] float roarVolume = 1f;

        [Header("Debug")]
        [SerializeField] bool debugMode;

        Vector3 currentVelocity;
        float currentSpeed;
        NavMeshAgent navMeshAgent;
        bool hasCaptured;
        Coroutine captureRoutine;
        PlayerController _targetPlayer;
        CinemachineImpulseSource _impulse;
        EnemyFootstepEmitter _footstepEmitter;
        bool _nearCatchPlayed;

        public bool IsChasing => isChasing;
        public bool HasCaptured => hasCaptured;
        public Transform Target => target;
        public float CurrentSpeed => currentSpeed;
        public float AggressiveChaseSpeed => aggressiveChaseSpeed;
        public Animator Animator => animator;
        public string RunAnimationBool => runAnimationBool;
        public string SpeedAnimationFloat => speedAnimationFloat;

        public System.Action OnPlayerCaptured;

        void Awake()
        {
            animator ??= GetComponent<Animator>();
            captureHandler ??= GetComponent<CaptureHandler>();
            _impulse = GetComponent<CinemachineImpulseSource>();
            _footstepEmitter = GetComponent<EnemyFootstepEmitter>();

            navMeshAgent = GetComponent<NavMeshAgent>();
            if (navMeshAgent != null)
            {
                navMeshAgent.speed = aggressiveChaseSpeed;
                navMeshAgent.acceleration = 9f;
                navMeshAgent.angularSpeed = 420f;
                navMeshAgent.stoppingDistance = minDistance;
                navMeshAgent.updateRotation = false;
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 1f;
                }
            }
        }

        void OnDisable()
        {
            if (captureRoutine != null)
            {
                StopCoroutine(captureRoutine);
                captureRoutine = null;
            }
        }

        void Update()
        {
            if (isChasing && target != null)
            {
                UpdateChase();
                UpdateNearCatchFx();
                UpdateAnimation();
                TryCapture();
            }

            if (hasCaptured)
                UpdateCaptureHoldPoint();
        }

        void UpdateChase()
        {
            float distanceToTarget = GetDistanceToPlayer();

            if (distanceToTarget < minDistance)
            {
                currentSpeed = 0f;
                if (navMeshAgent != null)
                {
                    navMeshAgent.isStopped = true;
                    navMeshAgent.ResetPath();
                }
                return;
            }

            currentSpeed = Mathf.Lerp(currentSpeed, GetChaseSpeed(distanceToTarget), Time.deltaTime * 5f);

            Vector3 targetPosition = target.position;
            Vector3 directionToTarget = (targetPosition - transform.position).normalized;
            directionToTarget.y = 0f;

            if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.speed = currentSpeed;
                navMeshAgent.acceleration = Mathf.Max(9f, currentSpeed * 2.2f);
                navMeshAgent.SetDestination(targetPosition);
            }
            else
            {
                Vector3 desiredPosition = transform.position + directionToTarget * currentSpeed * Time.deltaTime;

                if (useFixedHeight)
                    desiredPosition.y = fixedHeight;

                transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * movementSmoothness);
            }

            if (directionToTarget.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }

            if (debugMode)
            {
                Debug.DrawRay(transform.position, directionToTarget * 5f, Color.red);
                Debug.DrawLine(transform.position, target.position, Color.yellow);
            }
        }

        // tremor e grunhido quando ele quase encosta nela
        void UpdateNearCatchFx()
        {
            float distance = GetDistanceToPlayer();
            if (distance > NEAR_CATCH_FX_DISTANCE * 1.5f) _nearCatchPlayed = false;
            if (_nearCatchPlayed || distance > NEAR_CATCH_FX_DISTANCE) return;

            _nearCatchPlayed = true;
            if (_impulse != null) _impulse.GenerateImpulse();
            Audio.AudioManager.Instance?.PlayEvent(roarAudioEvent, transform.position);
        }

        float GetChaseSpeed(float distance)
        {
            float playerSpeed = _targetPlayer != null ? _targetPlayer.CurrentSpeed : 0f;
            if (playerSpeed < PLAYER_RUNNING_SPEED) return aggressiveChaseSpeed;
            return Mathf.Clamp(playerSpeed + (distance - CHASE_GAP) * 0.5f, playerSpeed * 0.75f, aggressiveChaseSpeed * 1.4f);
        }

        void UpdateAnimation()
        {
            if (animator == null) return;

            bool shouldRun = currentSpeed > 0.1f;
            float normalizedSpeed = currentSpeed / aggressiveChaseSpeed;

            if (!string.IsNullOrEmpty(runAnimationBool))
                animator.SetBool(runAnimationBool, shouldRun);

            if (!string.IsNullOrEmpty(speedAnimationFloat))
                animator.SetFloat(speedAnimationFloat, normalizedSpeed);
        }

        // chamado pelo evento de pisada da animacao de corrida
        public void PlayStepAnimationEvent() => _footstepEmitter?.PlayFootstep();

        void TryCapture()
        {
            if (hasCaptured || target == null) return;

            if (GetDistanceToPlayer() <= captureDistance)
                CapturePlayer();
        }

        void CapturePlayer()
        {
            hasCaptured = true;
            StopChase();

            if (animator != null && !string.IsNullOrEmpty(captureAnimationTrigger))
                animator.SetTrigger(captureAnimationTrigger);

            var playerController = target != null ? target.GetComponent<PlayerController>() : null;
            if (playerController != null && captureHandler != null && playerHoldPoint != null)
            {
                captureHandler.CapturePlayer(playerController, playerHoldPoint);
            }
            else
            {
                var mediator = target != null ? target.GetComponent<ChaseInputMediator>() : null;
                mediator?.DisableAllInputs();
            }

            if (captureRoutine != null)
                StopCoroutine(captureRoutine);
            captureRoutine = StartCoroutine(KillCapturedPlayerAfterDelay());

            OnPlayerCaptured?.Invoke();
        }

        IEnumerator KillCapturedPlayerAfterDelay()
        {
            yield return new WaitForSeconds(captureKillDelay);

            if (target != null)
            {
                var playerDeath = target.GetComponent<PlayerDeath>();
                playerDeath?.DieFromCapture();
            }

            if (captureHandler != null)
                captureHandler.ReleasePlayer(detachFromHoldPoint: false, restoreController: false, clearCapturedState: false);

            captureRoutine = null;
        }

        void UpdateCaptureHoldPoint()
        {
            if (captureHandler != null && captureHandler.IsHoldingPlayer && playerHoldPoint != null)
                captureHandler.UpdatePlayerPosition(playerHoldPoint);
        }

        public void AlignBehindTarget()
        {
            if (target == null) return;

            Vector3 spawnPosition = target.position + target.TransformDirection(startOffset);
            if (useFixedHeight)
                spawnPosition.y = fixedHeight;

            transform.position = spawnPosition;

            Vector3 lookDirection = (target.position - transform.position).normalized;
            lookDirection.y = 0f;
            if (lookDirection.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        public void StartChase()
        {
            if (target == null)
            {
                Debug.LogWarning("[ChaseEnemyController] Target nao definido, nao pode iniciar persecucao");
                return;
            }

            isChasing = true;
            hasCaptured = false;
            currentSpeed = 0f;

            AlignBehindTarget();

            if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(target.position);
            }
        }

        public void StopChase()
        {
            isChasing = false;
            currentSpeed = 0f;

            if (navMeshAgent != null)
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.ResetPath();
            }

            if (animator != null && !string.IsNullOrEmpty(runAnimationBool))
                animator.SetBool(runAnimationBool, false);
        }

        public void ResetCaptureState()
        {
            hasCaptured = false;
            if (captureRoutine != null)
            {
                StopCoroutine(captureRoutine);
                captureRoutine = null;
            }

            if (captureHandler != null && captureHandler.IsHoldingPlayer)
                captureHandler.ReleasePlayer();
        }

        public void PlayRoar()
        {
            if (!roarAudioEvent.Equals(default(Audio.AudioEvent)))
                Audio.AudioManager.Instance?.PlayEvent(roarAudioEvent, transform.position);

            if (animator != null && !string.IsNullOrEmpty(roarAnimationTrigger))
            {
                animator.ResetTrigger(roarAnimationTrigger);
                animator.SetTrigger(roarAnimationTrigger);
            }

            if (animator != null && !string.IsNullOrEmpty(roarStateName))
            {
                int roarStateHash = Animator.StringToHash(roarStateName);
                if (animator.HasState(roarLayerIndex, roarStateHash))
                {
                    animator.CrossFade(roarStateHash, roarCrossFade, roarLayerIndex, 0f);
                }
                else if (debugMode)
                {
                    Debug.LogWarning($"[ChaseEnemyController] Estado de roar '{roarStateName}' nao encontrado no layer {roarLayerIndex}");
                }
            }

            if (roarSound != null && audioSource != null)
                audioSource.PlayOneShot(roarSound, roarVolume);
        }

        public void SetRoarClip(AudioClip clip) => roarSound = clip;
        public void SetRoarEvent(Audio.AudioEvent audioEvent) => roarAudioEvent = audioEvent;

        public float GetDistanceToPlayer()
        {
            if (target == null) return float.MaxValue;
            return Vector3.Distance(transform.position, target.position);
        }

        public void SetSpeed(float speed)
        {
            currentSpeed = speed;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            _targetPlayer = newTarget != null ? newTarget.GetComponent<PlayerController>() : null;
        }

        protected void TeleportCloserToPlayer()
        {
            if (target == null) return;

            Vector3 teleportPosition = target.position + target.TransformDirection(startOffset * 0.75f);
            if (useFixedHeight)
                teleportPosition.y = fixedHeight;

            transform.position = Vector3.Lerp(transform.position, teleportPosition, Time.deltaTime * 2f);
        }

        void OnDrawGizmos()
        {
            if (target != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, target.position);

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, minDistance);

                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(transform.position, captureDistance);
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }

        void OnDrawGizmosSelected()
        {
            if (target != null)
            {
                Vector3 startPos = target.position + target.TransformDirection(startOffset);
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(startPos, 1f);
                Gizmos.DrawLine(target.position, startPos);
            }
        }
    }
}