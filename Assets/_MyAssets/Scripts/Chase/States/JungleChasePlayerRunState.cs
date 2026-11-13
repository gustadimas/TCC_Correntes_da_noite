using UnityEngine;

namespace CorrentesDaNoite.Chase.States
{
    public class JungleChasePlayerRunState : JungleChaseState
    {
        public JungleChasePlayerRunState(JungleChaseSequenceController controller) : base(controller) { }

        public override void Enter() => EnablePlayerControl();

        public override void Update()
        {
            if (controller.PathFollower != null && controller.PathFollower.PathCompleted) controller.TriggerEndSequence();
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
    }
}
