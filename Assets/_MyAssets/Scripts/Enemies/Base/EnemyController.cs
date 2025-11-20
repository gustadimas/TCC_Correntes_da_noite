using UnityEngine;
using CorrentesDaNoite.Checkpoint;

namespace CorrentesDaNoite.Enemies
{
    public class EnemyController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] protected EnemyMovement movement;
        [SerializeField] protected EnemyAnimationController animationController;
        [SerializeField] protected EnemyDetectionBase detection;
        [SerializeField] protected LightDetector lightDetector;
        [SerializeField] protected EnemySoundListener soundListener;
        [SerializeField] protected CaptureHandler captureHandler;

        [Header("Patrol Settings")]
        [SerializeField] protected Transform[] patrolPoints;
        [SerializeField] protected float patrolSpeed = 2f;

        [Header("Spotted Settings")]
        [SerializeField] protected float spottedDelay = 0.5f;
        [SerializeField] protected float turnSpeed = 5f;
        [SerializeField] protected float captureDistance = 2f;

        [Header("Chase Settings")]
        [SerializeField] protected float chaseSpeed = 7f;

        public float ChaseSpeed => chaseSpeed;

        [Header("Capture Settings")]
        [SerializeField] protected Transform playerHoldPoint;

        [Header("Lantern Settings")]
        [SerializeField] protected GameObject lanternGameObject;

        [Header("Sound Reaction Settings")]
        [SerializeField] protected bool enableSoundReactions = true;
        [SerializeField] protected float soundRotationSpeed = 3f;
        [SerializeField] protected float soundAlertDuration = 3f;
        [SerializeField] protected float minSoundDistanceToReact = 3f;
        [SerializeField] protected int soundsToTriggerChase = 3;
        [SerializeField] protected bool reactToWalkingSounds = true;
        [SerializeField] protected bool reactToRunningSounds = true;
        [SerializeField] protected bool reactToJumpingSounds = false;

        protected EnemyStateMachine _stateMachine;
        protected Transform _playerTransform;
        protected CorrentesDaNoite.Player.PlayerController _cachedPlayerController;

        protected bool _isRotatingToSound;
        protected Vector3 _targetSoundPosition;
        protected float _soundRotationTimer;
        protected float _maxSoundRotationTime = 2f;

        protected bool _isAlertedBySound;
        protected float _soundAlertTimer;
        protected int _soundHeardCount;
        protected Vector3 _lastSoundPosition;
        protected bool _isRotatingBackToPatrol;
        protected Vector3 _patrolReturnTarget;
        protected float _patrolRotationTimer;

        public System.Action OnPlayerCaptured;

        public EnemyMovement Movement => movement;
        public EnemyAnimationController AnimationController => animationController;
        public EnemyDetectionBase Detection => detection;
        public LightDetector LightDetector => lightDetector;
        public EnemySoundListener SoundListener => soundListener;
        public CaptureHandler CaptureHandler => captureHandler;
        public EnemyStateMachine StateMachine => _stateMachine;
        public Transform[] PatrolPoints => patrolPoints;
        public float PatrolSpeed => patrolSpeed;
        public float SpottedDelay => spottedDelay;
        public float TurnSpeed => turnSpeed;
        public float CaptureDistance => captureDistance;
        public Transform PlayerTransform => _playerTransform;
        public Transform PlayerHoldPoint => playerHoldPoint;
        public CorrentesDaNoite.Player.PlayerController Player => _cachedPlayerController;
        public bool IsRotatingToSound => _isRotatingToSound;
        public bool IsAlertedBySound => _isAlertedBySound;
        public int SoundHeardCount => _soundHeardCount;
        public bool IsRotatingBackToPatrol => _isRotatingBackToPatrol;

        protected virtual void Awake()
        {
            _stateMachine = new EnemyStateMachine();
            InitializeComponents();
        }

        protected virtual void Start()
        {
            InitializeStateMachine();
            CachePlayerReference();
        }

        protected virtual void OnEnable() => CheckpointManager.OnPlayerRespawned += ResetEnemy;

        protected virtual void OnDisable() =>CheckpointManager.OnPlayerRespawned -= ResetEnemy;

        protected virtual void CachePlayerReference()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
                _cachedPlayerController = player.GetComponent<CorrentesDaNoite.Player.PlayerController>();
            }
        }

        protected virtual void Update()
        {
            _stateMachine?.Update();
            UpdateSoundRotation();
            UpdateSoundAlert();
            UpdatePatrolRotation();
        }

        protected virtual void FixedUpdate() => _stateMachine?.FixedUpdate();

        protected virtual void UpdateSoundRotation()
        {
            if (!_isRotatingToSound) return;

            _soundRotationTimer += Time.deltaTime;

            Vector3 directionToSound = (_targetSoundPosition - transform.position).normalized;
            directionToSound.y = 0f;

            if (directionToSound != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToSound);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    soundRotationSpeed * Time.deltaTime
                );

                float angleToTarget = Quaternion.Angle(transform.rotation, targetRotation);

                if (angleToTarget < 5f || _soundRotationTimer >= _maxSoundRotationTime)
                {
                    _isRotatingToSound = false;
                    _soundRotationTimer = 0f;
                }
            }
            else
            {
                _isRotatingToSound = false;
                _soundRotationTimer = 0f;
            }
        }

        protected virtual void UpdateSoundAlert()
        {
            if (!_isAlertedBySound) return;

            _soundAlertTimer += Time.deltaTime;

            if (_soundAlertTimer >= soundAlertDuration)
            {
                _isAlertedBySound = false;
                _soundAlertTimer = 0f;
                _soundHeardCount = 0;

                if (_stateMachine.CurrentState is EnemyPatrolState patrolState && patrolPoints != null && patrolPoints.Length > 0)
                {
                    int currentIndex = GetCurrentPatrolIndex(patrolState);
                    if (currentIndex >= 0 && currentIndex < patrolPoints.Length)
                    {
                        _patrolReturnTarget = patrolPoints[currentIndex].position;
                        _isRotatingBackToPatrol = true;
                        _patrolRotationTimer = 0f;
                    }
                }
            }
        }

        protected virtual int GetCurrentPatrolIndex(EnemyPatrolState patrolState)
        {
            var field = patrolState.GetType().GetField("currentPointIndex",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
                return (int)field.GetValue(patrolState);
            return 0;
        }

        protected virtual void UpdatePatrolRotation()
        {
            if (!_isRotatingBackToPatrol) return;

            _patrolRotationTimer += Time.deltaTime;

            Vector3 directionToPatrol = (_patrolReturnTarget - transform.position).normalized;
            directionToPatrol.y = 0f;

            if (directionToPatrol != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPatrol);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    turnSpeed * Time.deltaTime
                );

                float angleToTarget = Quaternion.Angle(transform.rotation, targetRotation);
                if (angleToTarget < 5f || _patrolRotationTimer >= 2f)
                {
                    _isRotatingBackToPatrol = false;
                    _patrolRotationTimer = 0f;

                    if (_stateMachine.CurrentState is EnemyPatrolState) animationController?.SetWalking(true);
                }
            }
            else
            {
                _isRotatingBackToPatrol = false;
                _patrolRotationTimer = 0f;

                if (_stateMachine.CurrentState is EnemyPatrolState) animationController?.SetWalking(true);
            }
        }

        protected virtual void InitializeComponents()
        {
            if (movement == null) movement = GetComponent<EnemyMovement>();
            if (animationController == null) animationController = GetComponent<EnemyAnimationController>();
            if (detection == null) detection = GetComponent<EnemyDetectionBase>();
            if (lightDetector == null) lightDetector = GetComponent<LightDetector>();
            if (soundListener == null) soundListener = GetComponent<EnemySoundListener>();
            if (captureHandler == null) captureHandler = GetComponent<CaptureHandler>();
        }

        protected virtual void InitializeStateMachine()
        {
            if (patrolPoints != null && patrolPoints.Length > 0)
                _stateMachine.Initialize(new EnemyPatrolState(this, _stateMachine));
            else
                _stateMachine.Initialize(new EnemyIdleState(this, _stateMachine));
        }

        public virtual void OnPlayerDetectedByLight()
        {
            if (_stateMachine.CurrentState is EnemyPatrolState ||
                _stateMachine.CurrentState is EnemyIdleState)
            {
                CancelSoundRotation();
                _stateMachine.ChangeState(new EnemySpottedState(this, _stateMachine));
            }
        }

        public virtual void OnPlayerLostFromLight() { }

        public virtual void OnSoundHeard(Audio.SoundData sound, float distanceToSound)
        {
            if (!enableSoundReactions) return;

            if (distanceToSound < minSoundDistanceToReact)
                return;

            bool shouldReact = sound.soundType switch
            {
                Audio.SoundType.Walking => reactToWalkingSounds,
                Audio.SoundType.Running => reactToRunningSounds,
                Audio.SoundType.Jumping => reactToJumpingSounds,
                Audio.SoundType.Landing => reactToJumpingSounds,
                _ => false
            };

            if (!shouldReact) return;

            if (_stateMachine.CurrentState is EnemyPatrolState or EnemyIdleState)
            {
                _lastSoundPosition = sound.position;
                _soundHeardCount++;

                if (_soundHeardCount >= soundsToTriggerChase)
                {
                    CancelSoundRotation();
                    _stateMachine.ChangeState(new EnemySpottedState(this, _stateMachine));
                    return;
                }

                if (_isAlertedBySound) _soundAlertTimer = 0f;

                _isAlertedBySound = true;
                _soundAlertTimer = 0f;

                if (movement != null)
                    movement.Stop();

                if (animationController != null)
                {
                    animationController.SetWalking(false);
                    animationController.SetRunning(false);
                    animationController.ResetAllTriggers();
                }

                RotateTowardsSound(sound.position);
            }
            else if (_stateMachine.CurrentState is EnemyChaseState or EnemyCaptureState or EnemySpottedState)
            {
                return;
            }
        }

        protected virtual void RotateTowardsSound(Vector3 soundPosition)
        {
            _targetSoundPosition = soundPosition;
            _isRotatingToSound = true;
            _soundRotationTimer = 0f;
        }

        public virtual void CancelSoundRotation()
        {
            _isRotatingToSound = false;
            _soundRotationTimer = 0f;
            _isAlertedBySound = false;
            _soundAlertTimer = 0f;
            _soundHeardCount = 0;
            _isRotatingBackToPatrol = false;
            _patrolRotationTimer = 0f;
        }

        public virtual void SetLanternVisible(bool visible)
        {
            if (lanternGameObject != null)
                lanternGameObject.SetActive(visible);
        }

        protected virtual void ResetEnemy()
        {
            if (movement != null) movement.Stop();

            if (animationController != null)
                animationController.ResetToIdle();

            if (captureHandler != null && captureHandler.IsHoldingPlayer)
                captureHandler.ReleasePlayer();

            CancelSoundRotation();
            SetLanternVisible(true);

            if (_stateMachine == null)
                return;

            int closestPointIndex = GetClosestPatrolPointIndex();

            if (closestPointIndex >= 0)
            {
                TeleportToPatrolPoint(closestPointIndex);
                _stateMachine.ChangeState(new EnemyPatrolState(this, _stateMachine, closestPointIndex));
            }
            else
            {
                _stateMachine.ChangeState(new EnemyIdleState(this, _stateMachine));
            }
        }

        protected virtual int GetClosestPatrolPointIndex()
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
                return -1;

            int closestIndex = -1;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] == null) continue;

                float distance = Vector3.Distance(transform.position, patrolPoints[i].position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }

        protected virtual void TeleportToPatrolPoint(int patrolIndex)
        {
            if (patrolPoints == null || patrolIndex < 0 || patrolIndex >= patrolPoints.Length)
                return;

            Transform patrolPoint = patrolPoints[patrolIndex];
            if (patrolPoint == null)
                return;

            if (movement != null)
                movement.TeleportTo(patrolPoint.position);
            else
                transform.position = patrolPoint.position;

            int nextPointIndex = GetNextPatrolIndex(patrolIndex);
            if (nextPointIndex >= 0)
            {
                Vector3 lookDirection = patrolPoints[nextPointIndex].position - patrolPoint.position;
                lookDirection.y = 0f;

                if (lookDirection.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.LookRotation(lookDirection.normalized);
            }
        }

        protected virtual int GetNextPatrolIndex(int currentIndex)
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
                return -1;

            for (int offset = 1; offset <= patrolPoints.Length; offset++)
            {
                int candidateIndex = (currentIndex + offset) % patrolPoints.Length;
                if (patrolPoints[candidateIndex] != null)
                    return candidateIndex;
            }

            return -1;
        }

        protected virtual void OnValidate()
        {
            if (playerHoldPoint == null)
                Debug.LogWarning($"[{gameObject.name}] PlayerHoldPoint não está configurado! Crie um Empty GameObject filho e atribua.", this);
        }
    }
}
