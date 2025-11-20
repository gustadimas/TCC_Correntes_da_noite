using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public class EnemySpottedState : EnemyState
    {
        protected float _spottedTimer;

        public EnemySpottedState(EnemyController controller, EnemyStateMachine stateMachine) : base(controller, stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            _spottedTimer = 0f;

            controller.CancelSoundRotation();

            controller.Movement.Stop();
            controller.AnimationController.SetWalking(false);
            controller.AnimationController.SetRunning(false);
            controller.AnimationController.SetSpotted();
        }

        public override void Update()
        {
            base.Update();
            _spottedTimer += Time.deltaTime;
            RotateTowardsPlayer();

            if (_spottedTimer >= controller.SpottedDelay)
                stateMachine.ChangeState(new EnemyChaseState(controller, stateMachine));
        }

        protected void RotateTowardsPlayer()
        {
            if (controller.PlayerTransform == null) return;
            Vector3 directionToPlayer = (controller.PlayerTransform.position - controller.transform.position).normalized;
            directionToPlayer.y = 0f;
            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, targetRotation, controller.TurnSpeed * Time.deltaTime);
            }
        }
    }
}