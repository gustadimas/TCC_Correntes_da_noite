using UnityEngine;

namespace CorrentesDaNoite.Chase.States
{
    public class JungleChasePlayerRunState : JungleChaseState
    {
        float _nextStumbleTime;

        // se ela abre essa distancia tropeca e o tropeco dura ate ele chegar quase nela
        const float STUMBLE_DISTANCE = 9f;
        const float NEAR_CATCH_DISTANCE = 1.7f;
        const float STUMBLE_SPEED_MULTIPLIER = 0.3f;
        const float STUMBLE_COOLDOWN = 4f;
        const float MIN_STUMBLE_TIME = 0.5f;
        const float MAX_STUMBLE_TIME = 1.8f;
        const string STUMBLE_TRIGGER = "Stumble";

        public JungleChasePlayerRunState(JungleChaseSequenceController controller) : base(controller) { }

        public override void Enter()
        {
            EnablePlayerControl();
            _nextStumbleTime = Time.time + STUMBLE_COOLDOWN;
        }

        public override void Update()
        {
            if (controller.PathFollower != null && controller.PathFollower.PathCompleted) controller.TriggerEndSequence();
            else TryStumble();
        }

        void EnablePlayerControl()
        {
            controller.InputMediator?.EnablePlayerControl(keepForwardUntilInput: true);

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

        void TryStumble()
        {
            var enemy = controller.ChaseEnemyController;
            var stumble = controller.StumbleHandler;
            var player = controller.PlayerController;
            if (Time.time < _nextStumbleTime || enemy == null || stumble == null || player == null || !enemy.IsChasing || stumble.IsStumbling || !player.IsRunning) return;

            float distance = enemy.GetDistanceToPlayer();
            if (distance < STUMBLE_DISTANCE) return;

            float stumbleSpeed = player.CurrentSpeed * STUMBLE_SPEED_MULTIPLIER;
            float duration = Mathf.Clamp((distance - NEAR_CATCH_DISTANCE) / (enemy.AggressiveChaseSpeed - stumbleSpeed), MIN_STUMBLE_TIME, MAX_STUMBLE_TIME);
            stumble.TriggerStumble(STUMBLE_SPEED_MULTIPLIER, duration, STUMBLE_TRIGGER);
            _nextStumbleTime = Time.time + duration + STUMBLE_COOLDOWN;
        }
    }
}