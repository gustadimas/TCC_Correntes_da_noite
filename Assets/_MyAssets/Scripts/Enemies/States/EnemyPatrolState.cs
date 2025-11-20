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
                stateMachine.ChangeState(new EnemyIdleState(controller, stateMachine));
                return;
            }

            controller.AnimationController.ResetAllTriggers();
            controller.AnimationController.SetRunning(false);
            controller.Movement.SetSpeed(controller.PatrolSpeed);
            controller.Movement.SetAcceleration(16f);

            // Rotate towards patrol point before starting to move
            RotateTowardsCurrentPoint();

            controller.AnimationController.SetWalking(true);
            MoveToCurrentPoint();
        }

        public override void Update()
        {
            base.Update();

            if (_patrolPoints == null || _patrolPoints.Length == 0) return;

            bool isAlertedNow = controller.IsAlertedBySound;
            bool isRotatingBack = controller.IsRotatingBackToPatrol;

            // Pause patrol during alert, but don't block external state transitions
            if (isAlertedNow || isRotatingBack)
            {
                // Ensure animations are stopped while alerted
                controller.AnimationController.SetWalking(false);
                controller.AnimationController.SetRunning(false);

                if (isAlertedNow)
                    _wasAlertedLastFrame = true;
                // Don't return - allow base.Update() and state machine to continue
            }
            else
            {
                // Only process patrol logic when not alerted
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
            controller.Movement.Stop();
            controller.AnimationController.SetWalking(false);
        }

        protected bool HasReachedCurrentPoint()
        {
            return controller.Movement.HasReachedDestination(_arrivalThreshold);
        }

        protected void MoveToCurrentPoint()
        {
            if (_currentPointIndex < _patrolPoints.Length)
                controller.Movement.MoveTo(_patrolPoints[_currentPointIndex].position);
        }

        protected void RotateTowardsCurrentPoint()
        {
            if (_currentPointIndex >= _patrolPoints.Length || _patrolPoints[_currentPointIndex] == null)
                return;

            Vector3 direction = (_patrolPoints[_currentPointIndex].position - controller.transform.position).normalized;
            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                controller.transform.rotation = targetRotation;
            }
        }
    }
}