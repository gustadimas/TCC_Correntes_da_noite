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

            _controller.CancelSoundRotation();

            _controller.Movement.Stop();
            _controller.AnimationController.SetWalking(false);
            _controller.AnimationController.SetRunning(false);
            _controller.AnimationController.SetSpotted();
        }

        public override void Update()
        {
            base.Update();
            _spottedTimer += Time.deltaTime;
            RotateTowardsPlayer();

            if (_spottedTimer >= _controller.SpottedDelay)
                _stateMachine.ChangeState(new EnemyChaseState(_controller, _stateMachine));
        }

        protected void RotateTowardsPlayer()
        {
            if (_controller.PlayerTransform == null) return;
            Vector3 directionToPlayer = (_controller.PlayerTransform.position - _controller.transform.position).normalized;
            directionToPlayer.y = 0f;
            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                _controller.transform.rotation = Quaternion.Slerp(_controller.transform.rotation, targetRotation, _controller.TurnSpeed * Time.deltaTime);
            }
        }
    }
}