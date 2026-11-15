using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public class EnemySearchState : EnemyState
    {
        const float SEARCH_TIME = 4f;
        const float LOOK_ANGLE = 60f;
        const float LOOK_SPEED = 1.5f;

        Quaternion _startRotation;
        float _timer;
        bool _walking;
        Vector3 _target;

        public EnemySearchState(EnemyController controller, EnemyStateMachine stateMachine) : base(controller, stateMachine) { }

        // com destino ele vai ate o barulho da pedra antes de procurar
        public EnemySearchState(EnemyController controller, EnemyStateMachine stateMachine, Vector3 target) : base(controller, stateMachine)
        {
            _target = target;
            _walking = true;
        }

        public override void Enter()
        {
            _controller.SetLanternVisible(true);
            _controller.AnimationController.SetRunning(false);
            if (_walking && _controller.Movement.MoveTo(_target)) _controller.AnimationController.SetWalking(true);
            else StartLooking();
        }

        // perdeu ela de vista entao para e olha pros lados antes de voltar pra ronda
        public override void Update()
        {
            if (_walking)
            {
                if (_controller.Movement.HasReachedDestination()) StartLooking();
                return;
            }

            _timer += Time.deltaTime;
            _controller.transform.rotation = _startRotation * Quaternion.Euler(0f, Mathf.Sin(_timer * LOOK_SPEED) * LOOK_ANGLE, 0f);
            if (_timer >= SEARCH_TIME) _controller.ReturnToRoutine();
        }

        void StartLooking()
        {
            _walking = false;
            _controller.Movement.Stop();
            _controller.AnimationController.SetWalking(false);
            _startRotation = _controller.transform.rotation;
            _timer = 0f;
        }
    }
}