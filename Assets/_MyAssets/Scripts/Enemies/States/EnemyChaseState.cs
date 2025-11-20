using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public class EnemyChaseState : EnemyState
    {
        float _updatePathInterval = 0.1f;
        float _pathUpdateTimer;

        public EnemyChaseState(EnemyController controller, EnemyStateMachine stateMachine) : base(controller, stateMachine) { }

        public override void Enter()
        {
            base.Enter();

            // Clear any sound alert flags - chase has priority
            controller.CancelSoundRotation();

            controller.Movement.SetSpeed(controller.ChaseSpeed);
            controller.Movement.SetAcceleration(32f);
            controller.AnimationController.SetRunning(true);
            controller.SetLanternVisible(false);

            _pathUpdateTimer = 0f;
        }

        public override void Update()
        {
            base.Update();

            if (controller.PlayerTransform == null) return;

            _pathUpdateTimer += Time.deltaTime;
            if (_pathUpdateTimer >= _updatePathInterval)
            {
                controller.Movement.MoveTo(controller.PlayerTransform.position);
                _pathUpdateTimer = 0f;
            }

            float distanceToPlayer = Vector3.Distance(
                controller.transform.position,
                controller.PlayerTransform.position
            );

            if (distanceToPlayer <= controller.CaptureDistance)
                stateMachine.ChangeState(new EnemyCaptureState(controller, stateMachine));
        }

        public override void Exit()
        {
            base.Exit();

            controller.Movement.SetAcceleration(16f);
            controller.AnimationController.SetRunning(false);
        }
    }
}