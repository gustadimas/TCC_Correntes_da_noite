using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public class EnemyPatrolState : EnemyState
    {
        protected float _arrivalThreshold = 0.5f;

        protected Transform[] _patrolPoints;
        protected int _currentPointIndex;
        protected bool _wasAlertedLastFrame;
        protected float _pathCheckTimer;
        protected const float PathCheckDelay = 0.4f;

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
            _pathCheckTimer = 0f;
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
                    _pathCheckTimer = 0f;
                }
                else
                {
                    _pathCheckTimer += Time.deltaTime;
                    if (_pathCheckTimer >= PathCheckDelay && !_controller.Movement.HasValidPath)
                    {
                        int nextIndex = GetNextValidPointIndex(_currentPointIndex);
                        if (nextIndex >= 0)
                        {
                            _currentPointIndex = nextIndex;
                            MoveToCurrentPoint();
                            _pathCheckTimer = 0f;
                        }
                        else
                            _stateMachine.ChangeState(new EnemyIdleState(_controller, _stateMachine));
                    }
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
            if (_currentPointIndex >= _patrolPoints.Length || _patrolPoints[_currentPointIndex] == null)
                return;

            bool success = _controller.Movement.MoveTo(_patrolPoints[_currentPointIndex].position);
            _pathCheckTimer = 0f;

            if (!success)
            {
                int nextIndex = GetNextValidPointIndex(_currentPointIndex);
                if (nextIndex >= 0)
                {
                    _currentPointIndex = nextIndex;
                    success = _controller.Movement.MoveTo(_patrolPoints[_currentPointIndex].position);
                    _pathCheckTimer = 0f;

                    if (!success)
                        _stateMachine.ChangeState(new EnemyIdleState(_controller, _stateMachine));
                }
                else
                {
                    _stateMachine.ChangeState(new EnemyIdleState(_controller, _stateMachine));
                }
            }
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

        int GetNextValidPointIndex(int currentIndex)
        {
            if (_patrolPoints == null || _patrolPoints.Length == 0)
                return -1;

            int total = _patrolPoints.Length;
            for (int offset = 1; offset <= total; offset++)
            {
                int candidate = (currentIndex + offset) % total;
                if (_patrolPoints[candidate] != null)
                    return candidate;
            }

            return -1;
        }
    }
}