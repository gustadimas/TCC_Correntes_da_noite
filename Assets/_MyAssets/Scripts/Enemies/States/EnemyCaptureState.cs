using UnityEngine;
using CorrentesDaNoite.Player;

namespace CorrentesDaNoite.Enemies
{
    public class EnemyCaptureState : EnemyState
    {
        protected CaptureHandler _captureHandler;
        protected float _captureTimer;
        protected float _struggleDuration = 2.5f;
        protected bool _deathTriggered;

        // pe dela abaixo das maos dele no fim do grabbing
        static readonly Vector3 HANDS_TO_FEET = new Vector3(-0.06f, -0.46f, 0.02f);
        // onde as maos dele chegam quando ele abaixa
        const float REACH = 0.28f;
        const float APPROACH_TIME = 0.2f;
        Animator _playerAnimator;
        Vector3 _holdPointStart;
        Vector3 _approachFrom, _approachTo;
        Quaternion _turnFrom, _turnTo;

        public EnemyCaptureState(EnemyController controller, EnemyStateMachine stateMachine) : base(controller, stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            _captureTimer = 0f;
            _deathTriggered = false;

            _controller.Movement.Stop();
            _controller.AnimationController.SetRunning(false);
            _controller.AnimationController.SetWalking(false);
            _controller.AnimationController.SetCapture();

            if (_controller.PlayerHoldPoint != null) _holdPointStart = _controller.PlayerHoldPoint.localPosition;
            StartApproach();

            _captureHandler = _controller.CaptureHandler;
            if (_captureHandler != null && _controller.PlayerTransform != null)
            {
                PlayerController player = _controller.Player;

                if (player != null && _controller.PlayerHoldPoint != null)
                    _captureHandler.CapturePlayer(player, _controller.PlayerHoldPoint);
                _playerAnimator = player != null ? player.GetComponent<Animator>() : null;
            }

            _controller.OnPlayerCaptured?.Invoke();
            foreach (EnemyController enemy in Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None))
                if (enemy != _controller) enemy.GiveUpChase();
        }

        public override void Update()
        {
            base.Update();

            if (_deathTriggered) return;

            _captureTimer += Time.deltaTime;
            Approach();
            _controller.AnimationController.Grab(_playerAnimator, Mathf.Clamp01((_captureTimer - 0.15f) / 0.25f));

            if (_captureTimer >= _struggleDuration)
            {
                _deathTriggered = true;

                if (_captureHandler != null)
                    _captureHandler.ReleasePlayer(detachFromHoldPoint: false, restoreController: false, clearCapturedState: false);

                PlayerDeath playerDeath = _controller.Player?.GetComponent<PlayerDeath>();

                if (playerDeath != null)
                    playerDeath.DieFromCapture();

                return;
            }

            FollowHands();
            if (_captureHandler != null && _controller.PlayerHoldPoint != null)
                _captureHandler.UpdatePlayerPosition(_controller.PlayerHoldPoint);
        }

        // ele vira pra ela e da um passo ate as maos alcancarem onde ela esta
        void StartApproach()
        {
            Transform enemy = _controller.transform;
            _approachFrom = _approachTo = enemy.position;
            _turnFrom = _turnTo = enemy.rotation;
            if (_controller.PlayerTransform == null) return;

            Vector3 toPlayer = _controller.PlayerTransform.position - enemy.position;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude < 0.01f) return;

            _turnTo = Quaternion.LookRotation(toPlayer);
            float reach = REACH * enemy.lossyScale.z;
            if (toPlayer.magnitude > reach) _approachTo = enemy.position + toPlayer.normalized * (toPlayer.magnitude - reach);
        }

        void Approach()
        {
            float t = Mathf.Clamp01(_captureTimer / APPROACH_TIME);
            _controller.transform.rotation = Quaternion.Slerp(_turnFrom, _turnTo, t);
            _controller.Movement.MoveBy(Vector3.Lerp(_approachFrom, _approachTo, t) - _controller.transform.position);
        }

        // ela fica no chao enquanto ele abaixa e sobe junto com as maos dele
        void FollowHands()
        {
            Transform holdPoint = _controller.PlayerHoldPoint;
            Vector3 hands = _controller.AnimationController.AnimatedHands;
            if (holdPoint == null || hands == Vector3.zero) return;

            Vector3 local = _controller.transform.InverseTransformPoint(hands) + HANDS_TO_FEET;
            local.y = Mathf.Max(local.y, 0f);
            holdPoint.position = _controller.transform.TransformPoint(local);
        }

        public override void Exit()
        {
            base.Exit();

            if (_captureHandler != null)
                _captureHandler.ReleasePlayer();
            if (_controller.PlayerHoldPoint != null) _controller.PlayerHoldPoint.localPosition = _holdPointStart;
            _controller.AnimationController.Grab(null, 0f);

            _controller.AnimationController.SetWalking(false);
            _controller.AnimationController.SetRunning(false);
            _controller.AnimationController.ResetAllTriggers();
        }
    }
}