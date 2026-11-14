using UnityEngine;

namespace CorrentesDaNoite.Chase.States
{
    public class JungleChaseIntroState : JungleChaseState
    {
        float _stateTimer;

        public JungleChaseIntroState(JungleChaseSequenceController controller) : base(controller) { }

        public override void Enter()
        {
            _stateTimer = 0f;

            controller.InputMediator?.DisableAllInputs();
            PrepareEnemyBehindPlayer();

            if (controller.IntroCutsceneCamera != null)
                controller.SetActiveCamera(controller.IntroCutsceneCamera);

            if (controller.PlayerAnimator != null && !string.IsNullOrEmpty(controller.LookAroundAnimationTrigger))
            {
                controller.PlayerAnimator.SetTrigger(controller.LookAroundAnimationTrigger);
            }

            if (controller.IntroAmbientSound != null && controller.AudioSource != null)
            {
                controller.AudioSource.clip = controller.IntroAmbientSound;
                controller.AudioSource.Play();
            }
        }

        public override void Update()
        {
            _stateTimer += Time.deltaTime;

            if (_stateTimer >= controller.IntroCutsceneDuration)
            {
                controller.ChangeState(new JungleChaseEnemyRevealState(controller));
            }
        }

        void PrepareEnemyBehindPlayer()
        {
            if (controller.ChaseEnemy != null)
                controller.ChaseEnemy.SetActive(true);

            if (controller.ChaseEnemyController != null)
            {
                controller.ChaseEnemyController.SetTarget(controller.Player);
                controller.ChaseEnemyController.AlignBehindTarget();
                controller.ChaseEnemyController.StopChase();
            }
        }
    }
}
