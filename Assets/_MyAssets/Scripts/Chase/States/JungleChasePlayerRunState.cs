using UnityEngine;

namespace CorrentesDaNoite.Chase.States
{
    public class JungleChasePlayerRunState : JungleChaseState
    {
        float _stateTimer;
        bool _subscribedToSlowMo;

        public JungleChasePlayerRunState(JungleChaseSequenceController controller) : base(controller) { }

        public override void Enter()
        {
            _stateTimer = 0f;
            _subscribedToSlowMo = false;

            EnablePlayerControl();

            if (controller.DebugMode)
                Debug.Log("[JungleChasePlayerRun] Estado iniciado - Controle do jogador liberado");
        }

        public override void Update()
        {
            _stateTimer += Time.deltaTime;

            if (controller.ChaseEnemyController != null && controller.ChaseEnemyController.IsChasing && !controller.ChaseEnemyController.HasCaptured)
                MonitorEnemyDistance();

            if (controller.PathFollower != null && controller.PathFollower.PathCompleted)
                OnPathCompleted();
        }

        public override void Exit()
        {
            UnsubscribeSlowMoEnd();

            if (controller.DebugMode)
                Debug.Log($"[JungleChasePlayerRun] Estado finalizado - Tempo: {_stateTimer:F2}s");
        }

        void EnablePlayerControl()
        {
            if (!controller.EnableSlowMoCue)
                StopPlayerMovement();
            else
                SubscribeSlowMoEnd();

            controller.InputMediator?.EnablePlayerControl(keepForwardUntilInput: false);

            var dirManager = CorrentesDaNoite.Camera.CameraDirectionManager.Instance;
            if (dirManager != null && controller.GameplayChaseCamera != null)
                dirManager.SetDirectionFromCamera(controller.GameplayChaseCamera.transform, true);

            if (controller.PlayerController != null)
            {
                controller.PlayerController.SetDirectionalZonesEnabled(true);
                controller.PlayerController.SetCameraRelativeMovement(true);
                controller.PlayerController.SetInvertDirectionalHorizontal(true);
                controller.PlayerController.SetInputMultiplier(new Vector2(-1f, -1f));
            }

            controller.PathFollower?.SetAutoMove(false);
        }

        void SubscribeSlowMoEnd()
        {
            if (_subscribedToSlowMo) return;
            controller.OnSlowMoEnded += StopPlayerMovement;
            _subscribedToSlowMo = true;
        }

        void UnsubscribeSlowMoEnd()
        {
            if (!_subscribedToSlowMo) return;
            controller.OnSlowMoEnded -= StopPlayerMovement;
            _subscribedToSlowMo = false;
        }

        void StopPlayerMovement()
        {
            UnsubscribeSlowMoEnd();
            controller.PlayerController?.StopMovementImmediate();
            if (controller.PlayerAnimator != null)
            {
                controller.PlayerAnimator.SetBool("IsRunning", false);
                controller.PlayerAnimator.SetFloat("Speed", 0f);
            }
        }

        void MonitorEnemyDistance()
        {
            float distance = controller.ChaseEnemyController.GetDistanceToPlayer();

            if (controller.DebugMode && _stateTimer % 2f < Time.deltaTime)
                Debug.Log($"[JungleChasePlayerRun] Distancia do inimigo: {distance:F2}m");

            if (distance <= controller.EnemyCatchDistance)
                OnPlayerCaught();
        }

        void OnPlayerCaught()
        {
            if (controller.DebugMode)
                Debug.Log("[JungleChasePlayerRun] Player capturado pelo inimigo!");

            controller.StopSequence();

            if (controller.Player != null)
            {
                var playerDeath = controller.Player.GetComponent<Player.PlayerDeath>();
                playerDeath?.DieFromCapture();
            }
        }

        void OnPathCompleted()
        {
            if (controller.DebugMode)
                Debug.Log("[JungleChasePlayerRun] Caminho completo - acionando fim da perseguicao");

            controller.TriggerEndSequence();
        }
    }
}
