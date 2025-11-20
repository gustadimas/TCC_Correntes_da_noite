using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public class WatchGuardWalkingState : EnemyState
    {
        protected WatchGuardController _guardController;
        protected int _guardIndex;

        public WatchGuardWalkingState(WatchGuardController controller, EnemyStateMachine stateMachine, int guardIndex) : base(controller, stateMachine)
        {
            _guardController = controller;
            _guardIndex = guardIndex;
        }

        public override void Enter()
        {
            base.Enter();

            if (!_guardController.HasValidGuardPoints())
            {
                _stateMachine.ChangeState(new EnemyIdleState(_controller, _stateMachine));
                return;
            }

            _guardController.WatchAnimation?.SetWatching(false);
            if (_guardController.GuardLight != null)
                _guardController.GuardLight.SetLightActive(false);

            _controller.AnimationController.ResetAllTriggers();
            _controller.AnimationController.SetRunning(false);
            _controller.AnimationController.SetWalking(true);

            _controller.Movement.SetSpeed(_guardController.GuardMoveSpeed);
            _controller.Movement.SetAcceleration(16f);
            _controller.Movement.MoveTo(_guardController.GetGuardPointPosition(_guardIndex));
        }

        public override void Update()
        {
            base.Update();

            if (_guardController.HasReachedGuardPoint())
            {
                _controller.AnimationController.SetWalking(false);
                _stateMachine.ChangeState(new WatchGuardWatchingState(_guardController, _stateMachine, _guardIndex));
            }
        }

        public override void Exit()
        {
            base.Exit();
            _controller.Movement.Stop();
        }
    }
}