using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public class WatchGuardWatchingState : EnemyState
    {
        protected WatchGuardController guardController;
        protected int guardIndex;
        protected float lookTimer;

        public WatchGuardWatchingState(WatchGuardController controller, EnemyStateMachine stateMachine, int guardIndex) : base(controller, stateMachine)
        {
            guardController = controller;
            this.guardIndex = guardIndex;
        }

        public override void Enter()
        {
            base.Enter();

            controller.Movement.Stop();
            controller.AnimationController.SetWalking(false);
            controller.AnimationController.SetRunning(false);
            lookTimer = 0f;

            guardController.WatchAnimation?.SetWatching(true);
            guardController.WatchAnimation?.TriggerLook();

            if (guardController.GuardLight != null)
                guardController.GuardLight.SetLightActive(true);
        }

        public override void Update()
        {
            base.Update();

            Transform lookTarget = guardController.GetLookTargetForGuardPoint(guardIndex);
            if (lookTarget != null)
                RotateTowards(lookTarget.position);

            if (IsAlignedWithTarget(lookTarget))
            {
                lookTimer += Time.deltaTime;
                if (lookTimer >= guardController.LookDuration)
                {
                    int nextIndex = guardController.GetNextGuardIndex(guardIndex);
                    if (nextIndex >= 0)
                        stateMachine.ChangeState(new WatchGuardWalkingState(guardController, stateMachine, nextIndex));
                }
            }
            else 
                lookTimer = 0f;
        }

        public override void Exit()
        {
            base.Exit();
            guardController.WatchAnimation?.SetWatching(false);
            if (guardController.GuardLight != null)
                guardController.GuardLight.SetLightActive(false);
        }

        void RotateTowards(Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - controller.transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            controller.transform.rotation = Quaternion.RotateTowards(
                controller.transform.rotation,
                targetRotation,
                guardController.LookRotationSpeed * Time.deltaTime
            );
        }

        bool IsAlignedWithTarget(Transform lookTarget)
        {
            if (lookTarget == null)
                return true;

            Vector3 toTarget = lookTarget.position - controller.transform.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude < 0.001f)
                return true;

            float angle = Vector3.Angle(controller.transform.forward, toTarget);
            return angle <= guardController.LookAlignmentTolerance;
        }
    }
}