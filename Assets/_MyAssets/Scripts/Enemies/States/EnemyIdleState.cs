using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public class EnemyIdleState : EnemyState
    {
        protected float _idleTimeMin = 1f;
        protected float _idleTimeMax = 3f;

        protected float _currentIdleTime;
        protected float _idleTimer;

        public EnemyIdleState(EnemyController controller, EnemyStateMachine stateMachine) : base(controller, stateMachine) { }

        public override void Enter()
        {
            base.Enter();

            _currentIdleTime = Random.Range(_idleTimeMin, _idleTimeMax);
            _idleTimer = 0f;

            _controller.Movement.Stop();
            _controller.AnimationController.ResetAllTriggers();
            _controller.AnimationController.SetRunning(false);
            _controller.AnimationController.SetWalking(false);
        }

        public override void Update()
        {
            base.Update();

            if (_controller.IsAlertedBySound || _controller.IsRotatingBackToPatrol)
            {
                _controller.AnimationController.SetWalking(false);
                _controller.AnimationController.SetRunning(false);
            }
            else
            {
                _idleTimer += Time.deltaTime;

                if (_idleTimer >= _currentIdleTime)
                {
                    if (_controller.PatrolPoints != null && _controller.PatrolPoints.Length > 0)
                    {
                        int startIndex = _controller.ClosestPatrolPointIndex;
                        if (startIndex < 0) startIndex = 0;
                        _stateMachine.ChangeState(new EnemyPatrolState(_controller, _stateMachine, startIndex));
                    }
                }
            }
        }
    }
}