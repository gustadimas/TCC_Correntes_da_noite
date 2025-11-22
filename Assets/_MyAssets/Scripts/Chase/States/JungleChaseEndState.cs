using UnityEngine;

namespace CorrentesDaNoite.Chase.States
{
    public class JungleChaseEndState : JungleChaseState
    {
        float _stateTimer;
        bool _enemyStopped;
        bool _playerStopped;
        bool _transitionComplete;

        public JungleChaseEndState(JungleChaseSequenceController controller) : base(controller) { }

        public override void Enter()
        {
            _stateTimer = 0f;
            _enemyStopped = false;
            _playerStopped = false;
            _transitionComplete = false;

            StopChase();

            if (controller.DebugMode)
                Debug.Log("[JungleChaseEnd] Estado iniciado - Finalizando persecucao");
        }

        public override void Update()
        {
            _stateTimer += Time.deltaTime;

            if (!_enemyStopped && _stateTimer >= controller.EnemyStopDelay)
            {
                StopEnemy();
                _enemyStopped = true;
            }

            if (!_playerStopped && _stateTimer >= controller.PlayerStopDelay)
            {
                StopPlayer();
                _playerStopped = true;
            }

            if (!_transitionComplete && _stateTimer >= controller.EndSequenceDuration)
            {
                CompleteSequence();
                _transitionComplete = true;
            }
        }

        public override void Exit()
        {
            if (controller.DebugMode)
                Debug.Log($"[JungleChaseEnd] Estado finalizado - Tempo total: {_stateTimer:F2}s");
        }

        void StopChase()
        {
            controller.InputMediator?.DisableAllInputs();
            controller.LookBackController?.DisableLookBack();
        }

        void StopEnemy()
        {
            controller.ChaseEnemyController?.StopChase();

            if (!string.IsNullOrEmpty(controller.EnemyStopAnimationTrigger))
            {
                Animator enemyAnimator = controller.ChaseEnemy?.GetComponent<Animator>();
                enemyAnimator?.SetTrigger(controller.EnemyStopAnimationTrigger);
            }

            if (controller.EnemyStopSound != null && controller.AudioSource != null)
                controller.AudioSource.PlayOneShot(controller.EnemyStopSound);
        }

        void StopPlayer()
        {
            if (controller.PlayerAnimator != null)
            {
                controller.PlayerAnimator.SetBool("IsRunning", false);
                controller.PlayerAnimator.SetFloat("Speed", 0f);

                if (!string.IsNullOrEmpty(controller.PlayerVictoryAnimationTrigger))
                    controller.PlayerAnimator.SetTrigger(controller.PlayerVictoryAnimationTrigger);
            }

            if (controller.PlayerController != null)
                controller.PlayerController.enabled = false;

            if (controller.VictorySound != null && controller.AudioSource != null)
                controller.AudioSource.PlayOneShot(controller.VictorySound);
        }

        void CompleteSequence()
        {
            if (controller.DebugMode)
                Debug.Log("[JungleChaseEnd] Sequencia completa - Acionando fim de fase");

            controller.OnChaseCompleted?.Invoke();

            if (controller.PlayerController != null)
                controller.PlayerController.enabled = true;

            controller.InputMediator?.EnableAllInputs();

            if (controller.EndSequenceTeleportZone != null)
            {
                controller.EndSequenceTeleportZone.TeleportPlayer(controller.Player.gameObject);
            }
            else if (controller.DebugMode)
            {
                Debug.LogWarning("[JungleChaseEnd] Nenhum teleporte de fim configurado");
            }
        }
    }
}
