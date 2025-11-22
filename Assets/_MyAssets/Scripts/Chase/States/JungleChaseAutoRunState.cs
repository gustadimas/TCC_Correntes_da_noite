using UnityEngine;

namespace CorrentesDaNoite.Chase.States
{
    public class JungleChaseAutoRunState : JungleChaseState
    {
        float _stateTimer;
        float _distanceTraveled;
        Vector3 _lastPosition;
        bool _enemyStarted;
        bool _slowMoTriggered;

        const float AutoRunDurationOverride = 2f;
        const float AutoRunDistanceOverride = 8f;
        const float AutoRunMaxWaitOverride = 3f;

        public JungleChaseAutoRunState(JungleChaseSequenceController controller) : base(controller) { }

        public override void Enter()
        {
            _stateTimer = 0f;
            _distanceTraveled = 0f;
            _slowMoTriggered = false;
            _enemyStarted = controller.ChaseEnemyController != null && controller.ChaseEnemyController.IsChasing;

            if (controller.Player != null)
                _lastPosition = controller.Player.position;

            controller.InputMediator?.DisableAllInputs();
            controller.SetActiveCamera(controller.GameplayChaseCamera);
            EnableAutoRun();

            if (controller.DebugMode)
                Debug.Log("[JungleChaseAutoRun] Estado iniciado - corrida automatica");
        }

        public override void Update()
        {
            _stateTimer += Time.deltaTime;

            if (controller.Player != null)
            {
                float frameDistance = Vector3.Distance(controller.Player.position, _lastPosition);
                _distanceTraveled += frameDistance;
                _lastPosition = controller.Player.position;
            }

            if (!_enemyStarted && _stateTimer >= controller.EnemyStartChaseDelay)
            {
                StartEnemyChase();
                _enemyStarted = true;
            }

            bool shouldTransition = false;

            if (controller.UseDistanceForTransition && _distanceTraveled >= Mathf.Min(controller.AutoRunDistance, AutoRunDistanceOverride))
                shouldTransition = true;

            if (controller.UseTimeForTransition && _stateTimer >= Mathf.Min(controller.AutoRunDuration, AutoRunDurationOverride))
                shouldTransition = true;

            if (!controller.UseTimeForTransition && !controller.UseDistanceForTransition && !controller.UseWaypointForTransition)
            {
                if (_stateTimer >= AutoRunMaxWaitOverride)
                    shouldTransition = true;
            }

            if (controller.UseWaypointForTransition && controller.TransitionWaypoint != null)
            {
                float distanceToWaypoint = Vector3.Distance(controller.Player.position, controller.TransitionWaypoint.position);
                if (distanceToWaypoint <= controller.WaypointTransitionThreshold)
                    shouldTransition = true;
            }

            if (shouldTransition)
            {
                if (!_slowMoTriggered)
                {
                    controller.TriggerSlowMoCue();
                    _slowMoTriggered = true;
                }
                controller.ChangeState(new JungleChasePlayerRunState(controller));
            }
        }

        public override void Exit()
        {
            if (controller.DebugMode)
                Debug.Log($"[JungleChaseAutoRun] Estado finalizado - Distancia: {_distanceTraveled:F2}m, Tempo: {_stateTimer:F2}s");
        }

        void EnableAutoRun()
        {
            controller.InputMediator?.EnableAutoRun();

            var cam = controller.GameplayChaseCamera != null ? controller.GameplayChaseCamera.transform : null;
            var dirManager = CorrentesDaNoite.Camera.CameraDirectionManager.Instance;
            dirManager?.SetDirectionFromCamera(cam, invertForward: true);

            if (controller.PlayerController != null)
            {
                controller.PlayerController.SetDirectionalZonesEnabled(true);
                controller.PlayerController.SetCameraRelativeMovement(false);
                controller.PlayerController.SetInvertDirectionalHorizontal(true);
                controller.PlayerController.SetInputMultiplier(new Vector2(-1f, -1f));
                ForcePlayerMovement();
            }

            if (controller.PathFollower != null)
            {
                controller.PathFollower.SetAutoMove(true);
                controller.PathFollower.SetAutoMoveSpeed(controller.AutoRunSpeed);
            }
        }

        void ForcePlayerMovement()
        {
            if (controller.PlayerAnimator != null)
            {
                controller.PlayerAnimator.SetBool("IsRunning", true);
                controller.PlayerAnimator.SetFloat("Speed", 1f);
            }
        }

        void StartEnemyChase()
        {
            if (controller.ChaseEnemyController != null && !controller.ChaseEnemyController.IsChasing)
            {
                controller.ChaseEnemyController.SetTarget(controller.Player);
                controller.ChaseEnemyController.StartChase();
            }
        }
    }
}
