using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public class EnemyChaseState : EnemyState
    {
        float _updatePathInterval = 0.1f;
        float _pathUpdateTimer;

        public EnemyChaseState(EnemyController controller, EnemyStateMachine stateMachine) : base(controller, stateMachine) { }

        public override void Enter()
        {
            base.Enter();

            _controller.CancelSoundRotation();

            _controller.Movement.SetSpeed(_controller.ChaseSpeed);
            _controller.Movement.SetAcceleration(32f);
            _controller.AnimationController.SetRunning(true);
            _controller.SetLanternVisible(false);

            _pathUpdateTimer = 0f;
        }

        public override void Update()
        {
            base.Update();

            if (_controller.PlayerTransform == null) return;

            _pathUpdateTimer += Time.deltaTime;
            if (_pathUpdateTimer >= _updatePathInterval)
            {
                _controller.Movement.MoveTo(_controller.PlayerTransform.position);
                _pathUpdateTimer = 0f;
            }

            float distanceToPlayer = Vector3.Distance(
                _controller.transform.position,
                _controller.PlayerTransform.position
            );

            if (distanceToPlayer <= _controller.CaptureDistance)
                _stateMachine.ChangeState(new EnemyCaptureState(_controller, _stateMachine));
        }

        public override void Exit()
        {
            base.Exit();

            _controller.Movement.SetAcceleration(16f);
            _controller.AnimationController.SetRunning(false);
        }
    }
}