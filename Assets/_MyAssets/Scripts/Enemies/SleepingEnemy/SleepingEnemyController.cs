using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public class SleepingEnemyController : EnemyController
    {
        [Header("Sleeping Animations")]
        [SerializeField] protected SleepingEnemyAnimationController sleepingAnimationController;
        [SerializeField] protected string sleepingBoolParam = "isSleeping";
        [SerializeField] protected string startledTriggerParam = "Startled";
        [SerializeField] protected string idleReadyBoolParam = "isIdleReady";

        [Header("Sleeping Timers")]
        [SerializeField] protected float startledDuration = 1.5f;

        protected Vector3 _initialPosition;
        protected Quaternion _initialRotation;
        protected bool _hasInitialPose;

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

        protected override void Awake()
        {
            base.Awake();
            RecordInitialTransform();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            RecordInitialTransform();
        }

        protected override void InitializeStateMachine()
        {
            _stateMachine.Initialize(new SleepingEnemySleepingState(this, _stateMachine));
        }

        public void GoToSleeping() => _stateMachine.ChangeState(new SleepingEnemySleepingState(this, _stateMachine));
        public void GoToStartled() => _stateMachine.ChangeState(new SleepingEnemyStartledState(this, _stateMachine));
        public void GoToIdleReady() => _stateMachine.ChangeState(new SleepingEnemyIdleReadyState(this, _stateMachine));

        protected void RecordInitialTransform()
        {
            if (_hasInitialPose) return;
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
            _hasInitialPose = true;
        }

        protected override void ResetEnemy()
        {
            if (movement != null) movement.Stop();
            if (animationController != null) animationController.ResetToIdle();
            if (captureHandler != null && captureHandler.IsHoldingPlayer) captureHandler.ReleasePlayer();

            CancelSoundRotation();
            SetLanternVisible(true);

            if (_hasInitialPose)
            {
                if (movement != null)
                    movement.TeleportTo(_initialPosition);
                else
                    transform.position = _initialPosition;

                transform.rotation = _initialRotation;
            }

            if (_stateMachine == null) return;
            _stateMachine.ChangeState(new SleepingEnemySleepingState(this, _stateMachine));
        }
    }
}