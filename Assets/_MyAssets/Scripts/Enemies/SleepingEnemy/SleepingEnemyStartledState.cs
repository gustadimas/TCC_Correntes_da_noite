using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public class SleepingEnemyStartledState : EnemyState
    {
        protected float _timer;

        public SleepingEnemyStartledState(SleepingEnemyController controller, EnemyStateMachine stateMachine) : base(controller, stateMachine) { }

        protected SleepingEnemyController SleepingController => _controller as SleepingEnemyController;

        public override void Enter()
        {
            base.Enter();
            _timer = 0f;
            _controller.Movement.Stop();
            SleepingController?.SleepingAnimation?.ResetAllTriggers();
            SleepingController?.SleepingAnimation?.SetSleeping(false, SleepingController.SleepingBoolParam);
            SleepingController?.SleepingAnimation?.SetIdleReady(false, SleepingController.IdleReadyBoolParam);
            SleepingController?.SleepingAnimation?.TriggerStartled(SleepingController.StartledTriggerParam);
        }

        public override void Update()
        {
            base.Update();
            _timer += Time.deltaTime;
            if (_timer >= SleepingController.StartledDuration)
                _stateMachine.ChangeState(new SleepingEnemySleepingState(SleepingController, _stateMachine));
        }
    }
}