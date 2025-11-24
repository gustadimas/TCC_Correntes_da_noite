using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public class WatchGuardWatchingState : EnemyState
    {
        protected WatchGuardController _guardController;
        protected int _guardIndex;
        protected float _lookTimer;
        bool _lightActivated;

        public WatchGuardWatchingState(WatchGuardController controller, EnemyStateMachine stateMachine, int guardIndex) : base(controller, stateMachine)
        {
            _guardController = controller;
            _guardIndex = guardIndex;
        }

        public override void Enter()
        {
            base.Enter();

            _controller.Movement.Stop();
            _controller.AnimationController.SetWalking(false);
            _controller.AnimationController.SetRunning(false);
            _lookTimer = 0f;
            _lightActivated = false;

            _guardController.WatchAnimation?.SetWatching(true);
            _guardController.WatchAnimation?.TriggerLook();
        }

        public override void Update()
        {
            base.Update();

            Transform lookTarget = _guardController.GetLookTargetForGuardPoint(_guardIndex);
            if (lookTarget != null)
                RotateTowards(lookTarget.position);

            bool isAligned = IsAlignedWithTarget(lookTarget);

            if (!_lightActivated && isAligned && _guardController.GuardLight != null)
            {
                _guardController.GuardLight.SetLightActive(true);
                _lightActivated = true;
            }

            if (isAligned)
            {
                _lookTimer += Time.deltaTime;
                if (_lookTimer >= _guardController.LookDuration)
                {
                    int nextIndex = _guardController.GetNextGuardIndex(_guardIndex);
                    if (nextIndex >= 0)
                        _stateMachine.ChangeState(new WatchGuardWalkingState(_guardController, _stateMachine, nextIndex));
                }
            }
            else 
                _lookTimer = 0f;
        }

        public override void Exit()
        {
            base.Exit();
            _guardController.WatchAnimation?.SetWatching(false);
            if (_guardController.GuardLight != null)
                _guardController.GuardLight.SetLightActive(false);
        }

        void RotateTowards(Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - _controller.transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            _controller.transform.rotation = Quaternion.RotateTowards(
                _controller.transform.rotation,
                targetRotation,
                _guardController.LookRotationSpeed * Time.deltaTime
            );
        }

        bool IsAlignedWithTarget(Transform lookTarget)
        {
            if (lookTarget == null)
                return true;

            Vector3 toTarget = lookTarget.position - _controller.transform.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude < 0.001f)
                return true;

            float angle = Vector3.Angle(_controller.transform.forward, toTarget);
            return angle <= _guardController.LookAlignmentTolerance;
        }
    }
}