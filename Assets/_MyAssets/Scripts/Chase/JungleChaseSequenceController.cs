using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using CorrentesDaNoite.Chase.States;
using CorrentesDaNoite.Camera;
using CorrentesDaNoite.UI;
using CorrentesDaNoite.Audio;

namespace CorrentesDaNoite.Chase
{
    public class JungleChaseSequenceController : MonoBehaviour
    {
        [Header("Sequence Settings")]
        [SerializeField] bool autoStartOnEnable;
        [SerializeField] bool debugMode = true;
        [SerializeField] float delayBeforeStart = 1.5f;
        [SerializeField] bool waitForTeleportStart = true;
        [SerializeField] float teleportStartDelay = 1f;

        [Header("State Durations")]
        [SerializeField] float introCutsceneDuration = 4f;
        [SerializeField] float enemyRevealDuration = 6f;

        [Header("Reveal Timing")]
        [SerializeField] float lookBackDelay = 0.35f;
        [SerializeField] float enemyCameraCutDelay = 0.6f;
        [SerializeField] float chaseCameraCutDelay = 2.3f;
        [SerializeField] float enemyRunStartDelay = 1.4f;
        [SerializeField] float playerAutoRunDelay = 0.35f;
        [SerializeField] float roarLoopInterval = 0.9f;

        [Header("Auto Run Settings")]
        [SerializeField] float autoRunDuration = 5f;
        [SerializeField] float autoRunDistance = 30f;
        [SerializeField] float autoRunSpeed = 6f;
        [SerializeField] bool useTimeForTransition = true;
        [SerializeField] bool useDistanceForTransition;
        [SerializeField] bool useWaypointForTransition;
        [SerializeField] Transform transitionWaypoint;
        [SerializeField] float waypointTransitionThreshold = 2f;

        [Header("Enemy Chase Settings")]
        [SerializeField] float enemyStartChaseDelay = 1f;
        [SerializeField] float enemyCatchDistance = 1.5f;

        [Header("Player References")]
        [SerializeField] Transform player;
        [SerializeField] Animator playerAnimator;
        [SerializeField] ChaseInputMediator inputMediator;
        [SerializeField] Player.PlayerController playerController;
        [SerializeField] ChasePathFollower pathFollower;
        [SerializeField] ChaseLookBackController lookBackController;
        [SerializeField] ChaseStumbleHandler stumbleHandler;

        [Header("Enemy References")]
        [SerializeField] GameObject chaseEnemy;
        [SerializeField] ChaseEnemyController chaseEnemyController;

        [Header("Cameras")]
        [SerializeField] CinemachineCamera introCutsceneCamera;
        [SerializeField] CinemachineCamera enemyRevealCamera;
        [SerializeField] CinemachineCamera playerFaceCamera;
        [SerializeField] CinemachineCamera gameplayChaseCamera;
        [SerializeField, Tooltip("Camera padrao do jogador (usada durante fade/respawn)")] CinemachineCamera playerDefaultCamera;

        [Header("Camera Settings")]
        [SerializeField] int defaultCameraPriority = 10;
        [SerializeField] int activeCameraPriority = 20;
        [SerializeField] bool enableCameraShake = true;

        [Header("Animation Triggers")]
        [SerializeField] string lookAroundAnimationTrigger = "LookAround";
        [SerializeField] string lookBackAnimationTrigger = "LookBack";

        [Header("Audio")]
        [SerializeField] AudioClip introAmbientSound;
        [SerializeField] AudioClip soundBehindClip;
        [SerializeField] AudioClip enemyRoarSound;
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioStateController audioStateController;
        [SerializeField] string chaseMusicKey = "Chase";
        [SerializeField] string explorationMusicKey = "Game";
        [SerializeField] string menuMusicKey = "Menu";
        [SerializeField] float musicFadeTime = 1.5f;

        [Header("End Sequence Settings")]
        [SerializeField] float endSequenceDuration = 5f;
        [SerializeField] float enemyStopDelay = 0.5f;
        [SerializeField] float playerStopDelay = 1f;
        [SerializeField] string enemyStopAnimationTrigger = "Stop";
        [SerializeField] string playerVictoryAnimationTrigger = "Victory";
        [SerializeField] AudioClip enemyStopSound;
        [SerializeField] AudioClip victorySound;
        [SerializeField] Teleport.TeleportZone endSequenceTeleportZone;

        public System.Action OnChaseCompleted;

        [Header("Chase Direction")]
        [SerializeField] CameraDirection autoRunDirection = CameraDirection.South;

        [Header("Slow Motion Cue")]
        [SerializeField] bool enableSlowMoCue = true;
        [SerializeField] float slowMoScale = 0.5f;
        [SerializeField] float slowMoDuration = 0.75f;

        [Header("Tutorial/Prompts")]
        [SerializeField] TutorialPromptUI tutorialUI;
        [SerializeField] string slowMotionRunPrompt = "Segure Shift para correr!";
        [SerializeField] float slowMotionPromptDelay = 0.2f;

        [Header("Respawn Reset")]
        [SerializeField, Tooltip("Delay extra apos respawn antes de reativar o chase")] float respawnResetDelay = 1f;

        JungleChaseState currentState;
        bool sequenceStarted;
        Coroutine startSequenceRoutine;
        Coroutine slowMoRoutine;
        Vector3 playerStartPosition;
        Quaternion playerStartRotation;
        Vector3 enemyStartPosition;
        Quaternion enemyStartRotation;
        bool awaitingRespawn;
        Player.PlayerDeath playerDeath;
        Coroutine respawnResetRoutine;

        public System.Action OnSlowMoEnded;

        public bool DebugMode => debugMode;
        public bool EnableSlowMoCue => enableSlowMoCue;
        public float IntroCutsceneDuration => introCutsceneDuration;
        public float EnemyRevealDuration => enemyRevealDuration;
        public float LookBackDelay => lookBackDelay;
        public float EnemyCameraCutDelay => enemyCameraCutDelay;
        public float ChaseCameraCutDelay => chaseCameraCutDelay;
        public float EnemyRunStartDelay => enemyRunStartDelay;
        public float PlayerAutoRunDelay => playerAutoRunDelay;
        public float RoarLoopInterval => roarLoopInterval;
        public float AutoRunDuration => autoRunDuration;
        public float AutoRunDistance => autoRunDistance;
        public float AutoRunSpeed => autoRunSpeed;
        public bool UseTimeForTransition => useTimeForTransition;
        public bool UseDistanceForTransition => useDistanceForTransition;
        public bool UseWaypointForTransition => useWaypointForTransition;
        public Transform TransitionWaypoint => transitionWaypoint;
        public float WaypointTransitionThreshold => waypointTransitionThreshold;
        public float EnemyStartChaseDelay => enemyStartChaseDelay;
        public float EnemyCatchDistance => enemyCatchDistance;
        public Transform Player => player;
        public Animator PlayerAnimator => playerAnimator;
        public ChaseInputMediator InputMediator => inputMediator;
        public Player.PlayerController PlayerController => playerController;
        public ChasePathFollower PathFollower => pathFollower;
        public ChaseLookBackController LookBackController => lookBackController;
        public ChaseStumbleHandler StumbleHandler => stumbleHandler;
        public GameObject ChaseEnemy => chaseEnemy;
        public ChaseEnemyController ChaseEnemyController => chaseEnemyController;
        public CinemachineCamera IntroCutsceneCamera => introCutsceneCamera;
        public CinemachineCamera EnemyRevealCamera => enemyRevealCamera;
        public CinemachineCamera PlayerFaceCamera => playerFaceCamera;
        public CinemachineCamera GameplayChaseCamera => gameplayChaseCamera;
        public int DefaultCameraPriority => defaultCameraPriority;
        public int ActiveCameraPriority => activeCameraPriority;
        public bool EnableCameraShake => enableCameraShake;
        public string LookAroundAnimationTrigger => lookAroundAnimationTrigger;
        public string LookBackAnimationTrigger => lookBackAnimationTrigger;
        public AudioClip IntroAmbientSound => introAmbientSound;
        public AudioClip SoundBehindClip => soundBehindClip;
        public AudioClip EnemyRoarSound => enemyRoarSound;
        public AudioSource AudioSource => audioSource;
        public float EndSequenceDuration => endSequenceDuration;
        public float EnemyStopDelay => enemyStopDelay;
        public float PlayerStopDelay => playerStopDelay;
        public string EnemyStopAnimationTrigger => enemyStopAnimationTrigger;
        public string PlayerVictoryAnimationTrigger => playerVictoryAnimationTrigger;
        public AudioClip EnemyStopSound => enemyStopSound;
        public AudioClip VictorySound => victorySound;
        public Teleport.TeleportZone EndSequenceTeleportZone => endSequenceTeleportZone;
        public float TeleportStartDelay => teleportStartDelay;
        public string ChaseMusicKey => chaseMusicKey;
        public string ExplorationMusicKey => explorationMusicKey;
        public string MenuMusicKey => menuMusicKey;

        void Awake()
        {
            InitializeReferences();
            InitializeAudioSource();
            CacheStartPositions();
            SubscribeCaptureEvents();
            SubscribePlayerDeathEvents();

            if (chaseEnemy != null)
                chaseEnemy.SetActive(false);
        }

        void OnEnable()
        {
            sequenceStarted = false;
            if (autoStartOnEnable && !waitForTeleportStart)
                BeginSequenceAfterDelay(delayBeforeStart);
        }

        void OnDisable()
        {
            UnsubscribeCaptureEvents();
            UnsubscribePlayerDeathEvents();
        }

        void Update()
        {
            currentState?.Update();
        }

        void InitializeReferences()
        {
            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                    player = playerObj.transform;
            }

            if (player != null)
            {
                playerAnimator ??= player.GetComponent<Animator>();
                inputMediator ??= player.GetComponent<ChaseInputMediator>();
                playerController ??= player.GetComponent<Player.PlayerController>();
                pathFollower ??= player.GetComponent<ChasePathFollower>();
                lookBackController ??= player.GetComponent<ChaseLookBackController>();
                stumbleHandler ??= player.GetComponent<ChaseStumbleHandler>();
                playerDeath ??= player.GetComponent<Player.PlayerDeath>();
            }

            if (chaseEnemy != null && chaseEnemyController == null)
                chaseEnemyController = chaseEnemy.GetComponent<ChaseEnemyController>();
        }

        void CacheStartPositions()
        {
            if (player != null)
            {
                playerStartPosition = player.position;
                playerStartRotation = player.rotation;
            }

            if (chaseEnemy != null)
            {
                enemyStartPosition = chaseEnemy.transform.position;
                enemyStartRotation = chaseEnemy.transform.rotation;
            }
        }

        void SubscribeCaptureEvents()
        {
            if (chaseEnemyController != null)
                chaseEnemyController.OnPlayerCaptured += HandlePlayerCaptured;
        }

        void UnsubscribeCaptureEvents()
        {
            if (chaseEnemyController != null)
                chaseEnemyController.OnPlayerCaptured -= HandlePlayerCaptured;
        }

        void SubscribePlayerDeathEvents()
        {
            if (playerDeath != null)
                playerDeath.OnCaptureFinished += HandlePlayerRespawned;
        }

        void UnsubscribePlayerDeathEvents()
        {
            if (playerDeath != null)
                playerDeath.OnCaptureFinished -= HandlePlayerRespawned;
        }

        void InitializeAudioSource()
        {
            if (audioSource != null) return;

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f;
            }
        }

        void ApplyExplorationAudio()
        {
            if (audioStateController != null)
                audioStateController.SetExplorationState();

            MusicManager.GetOrCreate().PlayMusic(explorationMusicKey, musicFadeTime);
        }

        void ApplyChaseAudio()
        {
            if (audioStateController != null)
                audioStateController.SetChaseState();

            MusicManager.GetOrCreate().PlayMusic(chaseMusicKey, musicFadeTime);
        }

        public void ApplyMenuAudio()
        {
            if (audioStateController != null)
                audioStateController.SetMenuState();

            MusicManager.GetOrCreate().PlayMusic(menuMusicKey, musicFadeTime);
        }

        public void StartSequence()
        {
            if (debugMode)
                Debug.Log("[JungleChaseSequence] Sequencia iniciada");

            ResetAllCameras();
            ApplyChaseAudio();
            ChangeState(new JungleChaseIntroState(this));
        }

        public void BeginSequenceAfterDelay(float? customDelay = null)
        {
            if (sequenceStarted) return;
            float delay = customDelay ?? teleportStartDelay;

            if (startSequenceRoutine != null)
                StopCoroutine(startSequenceRoutine);

            startSequenceRoutine = StartCoroutine(StartSequenceAfterDelay(delay));
        }

        IEnumerator StartSequenceAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            StartSequence();
            sequenceStarted = true;
            startSequenceRoutine = null;
        }

        public void ChangeState(JungleChaseState newState)
        {
            currentState?.Exit();
            currentState = newState;
            currentState?.Enter();

            if (debugMode && currentState != null)
                Debug.Log($"[JungleChaseSequence] Estado mudou para: {currentState.GetType().Name}");
        }

        void ResetAllCameras()
        {
            if (introCutsceneCamera != null)
                introCutsceneCamera.Priority = defaultCameraPriority;
            if (enemyRevealCamera != null)
                enemyRevealCamera.Priority = defaultCameraPriority;
            if (playerFaceCamera != null)
                playerFaceCamera.Priority = defaultCameraPriority;
            if (gameplayChaseCamera != null)
                gameplayChaseCamera.Priority = defaultCameraPriority;
            if (playerDefaultCamera != null)
                playerDefaultCamera.Priority = defaultCameraPriority;
        }

        public void SetActiveCamera(CinemachineCamera targetCamera)
        {
            ResetAllCameras();
            if (targetCamera != null)
                targetCamera.Priority = activeCameraPriority;
        }

        public void StopSequence()
        {
            currentState?.Exit();
            currentState = null;

            inputMediator?.EnableAllInputs();

            if (debugMode)
                Debug.Log("[JungleChaseSequence] Sequencia interrompida");

            ResetTimeScale();
            ApplyExplorationAudio();
        }

        public void TriggerEndSequence()
        {
            if (currentState is JungleChaseEndState)
            {
                if (debugMode)
                    Debug.LogWarning("[JungleChaseSequence] Ja esta no estado de fim");
                return;
            }

            ChangeState(new JungleChaseEndState(this));

            if (debugMode)
                Debug.Log("[JungleChaseSequence] Sequencia de fim acionada");

            ApplyExplorationAudio();
        }

        public void TriggerSequenceAfterTeleport(float? customDelay = null)
        {
            BeginSequenceAfterDelay(customDelay ?? teleportStartDelay);
        }

        void OnDrawGizmos()
        {
            if (player != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(player.position, 1f);
                Gizmos.DrawLine(player.position, player.position + player.forward * 2f);
            }

            if (chaseEnemy != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(chaseEnemy.transform.position, 2f);
            }
        }

        void OnDrawGizmosSelected()
        {
            if (introCutsceneCamera != null && player != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(player.position, introCutsceneCamera.transform.position);
                Gizmos.DrawWireSphere(introCutsceneCamera.transform.position, 0.5f);
            }

            if (enemyRevealCamera != null && chaseEnemy != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(chaseEnemy.transform.position, enemyRevealCamera.transform.position);
                Gizmos.DrawWireSphere(enemyRevealCamera.transform.position, 0.5f);
            }

            if (playerFaceCamera != null && player != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(player.position, playerFaceCamera.transform.position);
                Gizmos.DrawWireSphere(playerFaceCamera.transform.position, 0.5f);
            }
        }

        public void TriggerSlowMoCue()
        {
            if (!enableSlowMoCue) return;

            if (slowMoRoutine != null)
                StopCoroutine(slowMoRoutine);

            slowMoRoutine = StartCoroutine(SlowMoRoutine());
        }

        IEnumerator SlowMoRoutine()
        {
            float originalScale = Time.timeScale;
            float originalFixedDelta = Time.fixedDeltaTime;

            Time.timeScale = slowMoScale;
            Time.fixedDeltaTime = originalFixedDelta * slowMoScale;

            ShowSlowMoPrompt();

            yield return new WaitForSecondsRealtime(slowMoDuration);

            Time.timeScale = originalScale;
            Time.fixedDeltaTime = originalFixedDelta;
            OnSlowMoEnded?.Invoke();
            slowMoRoutine = null;
        }

        void ShowSlowMoPrompt()
        {
            if (tutorialUI == null)
            {
                tutorialUI = FindFirstObjectByType<TutorialPromptUI>();
            }

            if (tutorialUI == null)
                return;

            if (slowMotionPromptDelay > 0f)
                StartCoroutine(DelayedPrompt());
            else
                tutorialUI.ShowPrompt(slowMotionRunPrompt);
        }

        IEnumerator DelayedPrompt()
        {
            yield return new WaitForSecondsRealtime(slowMotionPromptDelay);
            tutorialUI?.ShowPrompt(slowMotionRunPrompt);
        }

        void ResetTimeScale()
        {
            if (slowMoRoutine != null)
            {
                StopCoroutine(slowMoRoutine);
                slowMoRoutine = null;
            }
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }

        void HandlePlayerCaptured()
        {
            if (debugMode)
                Debug.Log("[JungleChaseSequence] Player capturado - reiniciando perseguicao");

            awaitingRespawn = true;
            StopSequence();
            ResetChaseSystems(true);
            SwitchToPlayerDefaultCamera();
        }

        void HandlePlayerRespawned()
        {
            if (!awaitingRespawn) return;

            if (debugMode)
                Debug.Log("[JungleChaseSequence] Respawn apos captura - reiniciando perseguicao");

            awaitingRespawn = false;
            if (respawnResetRoutine != null)
                StopCoroutine(respawnResetRoutine);
            respawnResetRoutine = StartCoroutine(ResetAfterRespawnDelay());
        }

        IEnumerator ResetAfterRespawnDelay()
        {
            if (respawnResetDelay > 0f)
                yield return new WaitForSeconds(respawnResetDelay);

            ResetChaseSystems(false);
            SwitchToPlayerDefaultCamera();
            gameObject.SetActive(false);
            gameObject.SetActive(true);
            respawnResetRoutine = null;
        }

        void ResetChaseSystems(bool captured)
        {
            if (captured)
            {
                pathFollower?.SetAutoMove(false);
                SetCameraDirectionToAutoRun();
                return;
            }

            pathFollower?.ResetPath();
            pathFollower?.SetAutoMove(false);

            if (chaseEnemy != null)
            {
                chaseEnemy.transform.SetPositionAndRotation(enemyStartPosition, enemyStartRotation);
                chaseEnemy.SetActive(false);
            }

            chaseEnemyController?.ResetCaptureState();
            chaseEnemyController?.SetTarget(player);

            SetCameraDirectionToAutoRun();
            ResetAnimations();
        }

        void SwitchToPlayerDefaultCamera()
        {
            ResetAllCameras();
            if (playerDefaultCamera != null)
                playerDefaultCamera.Priority = activeCameraPriority;
        }

        void SetCameraDirectionToAutoRun()
        {
            var dirManager = CameraDirectionManager.Instance;
            if (dirManager != null)
                dirManager.SetDirection(autoRunDirection);
        }

        void ResetAnimations()
        {
            if (playerAnimator != null)
            {
                playerAnimator.SetBool("IsRunning", false);
                playerAnimator.SetFloat("Speed", 0f);
            }

            if (chaseEnemyController?.Animator != null)
            {
                if (!string.IsNullOrEmpty(chaseEnemyController.RunAnimationBool))
                    chaseEnemyController.Animator.SetBool(chaseEnemyController.RunAnimationBool, false);
                if (!string.IsNullOrEmpty(chaseEnemyController.SpeedAnimationFloat))
                    chaseEnemyController.Animator.SetFloat(chaseEnemyController.SpeedAnimationFloat, 0f);
            }
        }
    }
}
