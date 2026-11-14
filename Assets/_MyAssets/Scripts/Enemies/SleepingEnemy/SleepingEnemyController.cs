using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public class SleepingEnemyController : EnemyController
    {
        [Header("Config")]
        [SerializeField] protected SleepingEnemyConfig sleepingEnemyConfig;

        [Header("Sleeping Animations")]
        [SerializeField] protected SleepingEnemyAnimationController sleepingAnimationController;
        [SerializeField] protected string sleepingBoolParam = "isSleeping";
        [SerializeField] protected string startledTriggerParam = "Startled";
        [SerializeField] protected string idleReadyBoolParam = "isIdleReady";

        [Header("Sleeping Timers")]
        [SerializeField] protected float startledDuration = 1.5f;

        public SleepingEnemyAnimationController SleepingAnimation => sleepingAnimationController;
        public string SleepingBoolParam => sleepingBoolParam;
        public string StartledTriggerParam => startledTriggerParam;
        public string IdleReadyBoolParam => idleReadyBoolParam;
        public float StartledDuration => startledDuration;

        public Transform SleepingPlayerHoldPoint => playerHoldPoint;

        protected override void InitializeComponents()
        {
            base.InitializeComponents();
            if (sleepingAnimationController == null)
                sleepingAnimationController = GetComponent<SleepingEnemyAnimationController>();
        }

        protected override void InitializeStateMachine()
        {
            _stateMachine.Initialize(new SleepingEnemySleepingState(this, _stateMachine));
        }

        public void GoToSleeping() => _stateMachine.ChangeState(new SleepingEnemySleepingState(this, _stateMachine));
        public void GoToStartled() => _stateMachine.ChangeState(new SleepingEnemyStartledState(this, _stateMachine));
        public void GoToIdleReady() => _stateMachine.ChangeState(new SleepingEnemyIdleReadyState(this, _stateMachine));

        protected override void ApplyConfig()
        {
            if (sleepingEnemyConfig != null) enemyConfig = sleepingEnemyConfig;

            if (enemyConfig is SleepingEnemyConfig cfg)
            {
                patrolSpeed = cfg.patrolSpeed;
                chaseSpeed = cfg.chaseSpeed;
                turnSpeed = cfg.turnSpeed;
                captureDistance = cfg.captureDistance;

                startledDuration = cfg.startledDuration;

                minSoundDistanceToReact = cfg.minSoundDistanceToReact;
                soundsToTriggerChase = cfg.soundsToTriggerChase;
                soundAlertDuration = cfg.soundAlertDuration;
                soundRotationSpeed = cfg.soundRotationSpeed;
                enableSoundReactions = cfg.enableSoundReactions;
                reactToWalkingSounds = cfg.reactToWalkingSounds;
                reactToRunningSounds = cfg.reactToRunningSounds;
                reactToJumpingSounds = cfg.reactToJumpingSounds;

                if (soundListener is SleepingEnemySoundListener sleepingListener)
                {
                    sleepingListener.ApplySleepingConfig(
                        cfg.extraHearingRadius,
                        cfg.sensitivityMultiplier,
                        cfg.silenceTimeToSleep
                    );
                }

                if (lightDetector != null)
                    lightDetector.SetLightRadius(cfg.lightRadius);

                if (soundListener != null)
                    soundListener.SetHearingRange(cfg.hearingRange);
            }
        }

        protected override void ResetEnemy()
        {
            if (movement != null) movement.Stop();
            if (animationController != null) animationController.ResetToIdle();
            if (captureHandler != null && captureHandler.IsHoldingPlayer) captureHandler.ReleasePlayer();

            CancelSoundRotation();
            ResetDetectionAfterRespawn();
            SetLanternVisible(true);

            if (_stateMachine == null) return;
            TeleportToStart();
            _stateMachine.ChangeState(new SleepingEnemySleepingState(this, _stateMachine));
        }
    }
}