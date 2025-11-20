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

            controller.Movement.Stop();
            controller.AnimationController.ResetAllTriggers();
            controller.AnimationController.SetRunning(false);
            controller.AnimationController.SetWalking(false);
        }

        public override void Update()
        {
            base.Update();

            // Pause idle timer during alert, but don't block state machine
            if (controller.IsAlertedBySound || controller.IsRotatingBackToPatrol)
            {
                // Ensure animations are stopped while alerted (should already be idle, but enforce)
                controller.AnimationController.SetWalking(false);
                controller.AnimationController.SetRunning(false);
            }
            else
            {
                _idleTimer += Time.deltaTime;

                if (_idleTimer >= _currentIdleTime)
                {
                    if (controller.PatrolPoints != null && controller.PatrolPoints.Length > 0)
                        stateMachine.ChangeState(new EnemyPatrolState(controller, stateMachine));
                }
            }
        }
    }
}