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

            _controller.Movement.Stop();
            _controller.AnimationController.SetRunning(false);
            _controller.AnimationController.SetWalking(false);
            _controller.AnimationController.SetCapture();

            _captureHandler = _controller.CaptureHandler;
            if (_captureHandler != null && _controller.PlayerTransform != null)
            {
                PlayerController player = _controller.Player;

                if (player != null && _controller.PlayerHoldPoint != null)
                    _captureHandler.CapturePlayer(player, _controller.PlayerHoldPoint);
            }

            _controller.OnPlayerCaptured?.Invoke();
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

                PlayerDeath playerDeath = _controller.Player?.GetComponent<PlayerDeath>();

                if (playerDeath != null)
                    playerDeath.DieFromCapture();

                return;
            }

            if (_captureHandler != null && _controller.PlayerHoldPoint != null)
                _captureHandler.UpdatePlayerPosition(_controller.PlayerHoldPoint);
        }

        public override void Exit()
        {
            base.Exit();

            if (_captureHandler != null)
                _captureHandler.ReleasePlayer();

            _controller.AnimationController.SetWalking(false);
            _controller.AnimationController.SetRunning(false);
            _controller.AnimationController.ResetAllTriggers();
        }
    }
}