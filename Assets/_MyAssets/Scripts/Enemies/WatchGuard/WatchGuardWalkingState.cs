using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public class WatchGuardWalkingState : EnemyState
    {
        protected WatchGuardController _guardController;
        protected int _guardIndex;
        float _pathCheckTimer;
        const float PathCheckDelay = 0.4f;

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
            bool success = _controller.Movement.MoveTo(_guardController.GetGuardPointPosition(_guardIndex));
            _pathCheckTimer = 0f;

            if (!success)
            {
                int nextIndex = _guardController.GetNextGuardIndex(_guardIndex);
                if (nextIndex >= 0)
                    _stateMachine.ChangeState(new WatchGuardWalkingState(_guardController, _stateMachine, nextIndex));
                else
                    _stateMachine.ChangeState(new EnemyIdleState(_controller, _stateMachine));
            }
        }

        public override void Update()
        {
            base.Update();

            _pathCheckTimer += Time.deltaTime;

            if (_pathCheckTimer >= PathCheckDelay && !_controller.Movement.HasValidPath)
            {
                int nextIndex = _guardController.GetNextGuardIndex(_guardIndex);
                if (nextIndex >= 0)
                {
                    _stateMachine.ChangeState(new WatchGuardWalkingState(_guardController, _stateMachine, nextIndex));
                    return;
                }

                _stateMachine.ChangeState(new EnemyIdleState(_controller, _stateMachine));
                return;
            }

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