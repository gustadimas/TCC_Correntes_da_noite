using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public class EnemyPatrolState : EnemyState
    {
        protected float _arrivalThreshold = 0.5f;

        protected Transform[] _patrolPoints;
        protected int _currentPointIndex;
        protected bool _wasAlertedLastFrame;

        public EnemyPatrolState(EnemyController controller, EnemyStateMachine stateMachine, int startIndex = 0) : base(controller, stateMachine)
        {
            _patrolPoints = controller.PatrolPoints;
            _currentPointIndex = startIndex;
        }

        public override void Enter()
        {
            base.Enter();

            if (_patrolPoints == null || _patrolPoints.Length == 0)
            {
                _stateMachine.ChangeState(new EnemyIdleState(_controller, _stateMachine));
                return;
            }

            _controller.AnimationController.ResetAllTriggers();
            _controller.AnimationController.SetRunning(false);
            _controller.Movement.SetSpeed(_controller.PatrolSpeed);
            _controller.Movement.SetAcceleration(16f);

            RotateTowardsCurrentPoint();

            _controller.AnimationController.SetWalking(true);
            MoveToCurrentPoint();
        }

        public override void Update()
        {
            base.Update();

            if (_patrolPoints == null || _patrolPoints.Length == 0) return;

            bool isAlertedNow = _controller.IsAlertedBySound;
            bool isRotatingBack = _controller.IsRotatingBackToPatrol;

            if (isAlertedNow || isRotatingBack)
            {
                _controller.AnimationController.SetWalking(false);
                _controller.AnimationController.SetRunning(false);

                if (isAlertedNow)
                    _wasAlertedLastFrame = true;
            }
            else
            {
                if (_wasAlertedLastFrame)
                {
                    _wasAlertedLastFrame = false;
                    MoveToCurrentPoint();
                    return;
                }

                if (HasReachedCurrentPoint())
                {
                    _currentPointIndex++;

                    if (_currentPointIndex >= _patrolPoints.Length)
                        _currentPointIndex = 0;

                    MoveToCurrentPoint();
                }
            }
        }

        public override void Exit()
        {
            base.Exit();
            _controller.Movement.Stop();
            _controller.AnimationController.SetWalking(false);
        }

        protected bool HasReachedCurrentPoint()
        {
            return _controller.Movement.HasReachedDestination(_arrivalThreshold);
        }

        protected void MoveToCurrentPoint()
        {
            if (_currentPointIndex < _patrolPoints.Length)
                _controller.Movement.MoveTo(_patrolPoints[_currentPointIndex].position);
        }

        protected void RotateTowardsCurrentPoint()
        {
            if (_currentPointIndex >= _patrolPoints.Length || _patrolPoints[_currentPointIndex] == null)
                return;

            Vector3 direction = (_patrolPoints[_currentPointIndex].position - _controller.transform.position).normalized;
            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                _controller.transform.rotation = targetRotation;
            }
        }
    }
}