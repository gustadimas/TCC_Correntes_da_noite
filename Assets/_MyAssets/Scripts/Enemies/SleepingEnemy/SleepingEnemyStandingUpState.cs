using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public class SleepingEnemyStandingUpState : EnemyState
    {
        public SleepingEnemyStandingUpState(SleepingEnemyController controller, EnemyStateMachine stateMachine) : base(controller, stateMachine) { }

        protected SleepingEnemyController SleepingController => _controller as SleepingEnemyController;

        public override void Enter()
        {
            base.Enter();
            _controller.Movement.Stop();
            SleepingController?.SleepingAnimation?.ResetAllTriggers();
            SleepingController?.SleepingAnimation?.SetSleeping(false, SleepingController.SleepingBoolParam);
            SleepingController?.SleepingAnimation?.SetIdleReady(false, SleepingController.IdleReadyBoolParam);
            _stateMachine.ChangeState(new SleepingEnemyIdleReadyState(SleepingController, _stateMachine));
        }
    }
}