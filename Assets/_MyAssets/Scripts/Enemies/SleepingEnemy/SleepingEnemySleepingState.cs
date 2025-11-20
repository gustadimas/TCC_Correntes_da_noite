using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public class SleepingEnemySleepingState : EnemyState
    {
        public SleepingEnemySleepingState(SleepingEnemyController controller, EnemyStateMachine stateMachine) : base(controller, stateMachine) { }

        protected SleepingEnemyController SleepingController => _controller as SleepingEnemyController;

        public override void Enter()
        {
            base.Enter();
            _controller.Movement.Stop();
            SleepingController?.SleepingAnimation?.ResetAllTriggers();
            SleepingController?.SleepingAnimation?.SetRunning(false);
            SleepingController?.SleepingAnimation?.SetWalking(false);
            SleepingController?.SleepingAnimation?.SetSleeping(true, SleepingController.SleepingBoolParam);
            SleepingController?.SleepingAnimation?.SetIdleReady(false, SleepingController.IdleReadyBoolParam);
        }

        public override void Exit()
        {
            base.Exit();
            SleepingController?.SleepingAnimation?.SetSleeping(false, SleepingController.SleepingBoolParam);
        }
    }
}