using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public class WatchGuardWalkingState : EnemyState
    {
        protected WatchGuardController guardController;
        protected int guardIndex;

        public WatchGuardWalkingState(WatchGuardController controller, EnemyStateMachine stateMachine, int guardIndex) : base(controller, stateMachine)
        {
            guardController = controller;
            this.guardIndex = guardIndex;
        }

        public override void Enter()
        {
            base.Enter();

            if (!guardController.HasValidGuardPoints())
            {
                stateMachine.ChangeState(new EnemyIdleState(controller, stateMachine));
                return;
            }

            guardController.WatchAnimation?.SetWatching(false);
            if (guardController.GuardLight != null)
                guardController.GuardLight.SetLightActive(false);

            controller.AnimationController.ResetAllTriggers();
            controller.AnimationController.SetRunning(false);
            controller.AnimationController.SetWalking(true);

            controller.Movement.SetSpeed(guardController.GuardMoveSpeed);
            controller.Movement.SetAcceleration(16f);
            controller.Movement.MoveTo(guardController.GetGuardPointPosition(guardIndex));
        }

        public override void Update()
        {
            base.Update();

            if (guardController.HasReachedGuardPoint())
            {
                controller.AnimationController.SetWalking(false);
                stateMachine.ChangeState(new WatchGuardWatchingState(guardController, stateMachine, guardIndex));
            }
        }

        public override void Exit()
        {
            base.Exit();
            controller.Movement.Stop();
        }
    }
}