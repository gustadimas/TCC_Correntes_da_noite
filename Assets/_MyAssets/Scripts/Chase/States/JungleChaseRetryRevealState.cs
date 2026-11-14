using UnityEngine;
using Unity.Cinemachine;

namespace CorrentesDaNoite.Chase.States
{
    public class JungleChaseRetryRevealState : JungleChaseState
    {
        float _timer;
        bool _roarPlayed;

        const float ROAR_DELAY = 0.4f;
        const float RUN_DELAY = 2f;
        const string LOOK_BACK_STATE = "LookingBack";

        public JungleChaseRetryRevealState(JungleChaseSequenceController controller) : base(controller) { }

        public override void Enter()
        {
            controller.InputMediator?.DisableAllInputs();
            controller.SetActiveCamera(controller.GameplayChaseCamera);
            if (controller.ChaseEnemy != null) controller.ChaseEnemy.SetActive(true);

            if (controller.ChaseEnemyController != null)
            {
                controller.ChaseEnemyController.SetTarget(controller.Player);
                controller.ChaseEnemyController.AlignBehindTarget();
            }

            if (controller.PlayerAnimator != null) controller.PlayerAnimator.SetTrigger(controller.LookBackAnimationTrigger);
        }

        public override void Update()
        {
            _timer += Time.deltaTime;
            if (!_roarPlayed && _timer >= ROAR_DELAY) PlayRoar();

            // espera ela terminar de olhar pra tras senao ela desliza parada no auto run
            if (_timer < RUN_DELAY || IsLookingBack()) return;

            // ja sai correndo daqui pro auto run nao teleportar ele de novo atras dela
            if (controller.ChaseEnemyController != null) controller.ChaseEnemyController.StartChase();
            controller.ChangeState(new JungleChaseAutoRunState(controller));
        }

        bool IsLookingBack() => controller.PlayerAnimator != null && controller.PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName(LOOK_BACK_STATE);

        void PlayRoar()
        {
            _roarPlayed = true;
            if (controller.ChaseEnemyController == null) return;

            controller.ChaseEnemyController.PlayRoar();
            if (controller.EnableCameraShake && controller.ChaseEnemyController.TryGetComponent(out CinemachineImpulseSource impulse)) impulse.GenerateImpulse();
        }
    }
}