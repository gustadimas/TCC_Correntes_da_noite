using UnityEngine;
using CorrentesDaNoite.Player;

namespace CorrentesDaNoite.Enemies
{
    public class EnemyCaptureState : EnemyState
    {
        protected CaptureHandler _captureHandler;
        protected float _captureTimer;
        protected float _struggleDuration = 2.5f;
        protected bool _deathTriggered;

        public EnemyCaptureState(EnemyController controller, EnemyStateMachine stateMachine) : base(controller, stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            _captureTimer = 0f;
            _deathTriggered = false;

            controller.Movement.Stop();
            controller.AnimationController.SetRunning(false);
            controller.AnimationController.SetWalking(false);
            controller.AnimationController.SetCapture();

            _captureHandler = controller.CaptureHandler;
            if (_captureHandler != null && controller.PlayerTransform != null)
            {
                PlayerController player = controller.Player;

                if (player != null && controller.PlayerHoldPoint != null)
                    _captureHandler.CapturePlayer(player, controller.PlayerHoldPoint);
            }

            controller.OnPlayerCaptured?.Invoke();
        }

        public override void Update()
        {
            base.Update();

            if (_deathTriggered) return;

            _captureTimer += Time.deltaTime;

            if (_captureTimer >= _struggleDuration)
            {
                _deathTriggered = true;

                if (_captureHandler != null)
                    _captureHandler.ReleasePlayer();

                PlayerDeath playerDeath = controller.Player?.GetComponent<PlayerDeath>();

                if (playerDeath != null)
                    playerDeath.DieFromCapture();

                return;
            }

            if (_captureHandler != null && controller.PlayerHoldPoint != null)
                _captureHandler.UpdatePlayerPosition(controller.PlayerHoldPoint);
        }

        public override void Exit()
        {
            base.Exit();

            if (_captureHandler != null)
                _captureHandler.ReleasePlayer();

            controller.AnimationController.SetWalking(false);
            controller.AnimationController.SetRunning(false);
            controller.AnimationController.ResetAllTriggers();
        }
    }
}