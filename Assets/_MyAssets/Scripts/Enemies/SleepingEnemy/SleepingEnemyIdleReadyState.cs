using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public class SleepingEnemyIdleReadyState : EnemyState
    {
        public SleepingEnemyIdleReadyState(SleepingEnemyController controller, EnemyStateMachine stateMachine) : base(controller, stateMachine) { }

        protected SleepingEnemyController SleepingController => _controller as SleepingEnemyController;

        public override void Enter()
        {
            base.Enter();
            _controller.Movement.Stop();
            SleepingController?.SleepingAnimation?.ResetAllTriggers();
            SleepingController?.SleepingAnimation?.SetSleeping(false, SleepingController.SleepingBoolParam);
            SleepingController?.SleepingAnimation?.SetIdleReady(true, SleepingController.IdleReadyBoolParam);

            _stateMachine.ChangeState(new EnemyChaseState(_controller, _stateMachine));
        }

        public override void Exit() { }
    }
}