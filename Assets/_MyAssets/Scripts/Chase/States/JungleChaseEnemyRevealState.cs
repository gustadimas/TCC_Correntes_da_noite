using UnityEngine;

namespace CorrentesDaNoite.Chase.States
{
    public class JungleChaseEnemyRevealState : JungleChaseState
    {
        float _stateTimer;
        bool _lookBackTriggered;
        bool _enemyCameraActive;
        bool _playerFaceCameraActive;
        bool _chaseCameraActive;
        bool _enemyRunning;
        bool _autoRunQueued;
        float _enemyCameraStartTime = -1f;
        float _playerFaceCameraStartTime = -1f;
        bool _roarPlayed;

        const float EnemyCameraHoldDuration = 2.5f;
        const float PlayerFaceCameraDuration = 1.2f;

        public JungleChaseEnemyRevealState(JungleChaseSequenceController controller) : base(controller) { }

        public override void Enter()
        {
            _stateTimer = 0f;
            _lookBackTriggered = false;
            _enemyCameraActive = false;
            _playerFaceCameraActive = false;
            _chaseCameraActive = false;
            _enemyRunning = false;
            _autoRunQueued = false;
            _enemyCameraStartTime = -1f;
            _playerFaceCameraStartTime = -1f;
            _roarPlayed = false;

            EnsureEnemyReady();
            ConfigureEnemyAudio();
        }

        public override void Update()
        {
            _stateTimer += Time.deltaTime;

            if (!_lookBackTriggered && _stateTimer >= controller.LookBackDelay)
            {
                TriggerLookBack();
                _lookBackTriggered = true;
            }

            if (!_enemyCameraActive && _stateTimer >= controller.EnemyCameraCutDelay)
            {
                controller.SetActiveCamera(controller.EnemyRevealCamera);
                PlaySingleRoar();
                _enemyCameraActive = true;
                _enemyCameraStartTime = _stateTimer;
            }

            if (_enemyCameraActive && !_playerFaceCameraActive && _enemyCameraStartTime >= 0f &&
                _stateTimer - _enemyCameraStartTime >= EnemyCameraHoldDuration)
            {
                CutToPlayerFace();
                _playerFaceCameraActive = true;
                _playerFaceCameraStartTime = _stateTimer;
            }

            if (_playerFaceCameraActive && !_chaseCameraActive && _playerFaceCameraStartTime >= 0f &&
                _stateTimer - _playerFaceCameraStartTime >= PlayerFaceCameraDuration)
            {
                controller.SetActiveCamera(controller.GameplayChaseCamera);
                _chaseCameraActive = true;
            }

            float minRunTime = Mathf.Max(controller.EnemyRunStartDelay,
                _enemyCameraStartTime < 0f || _playerFaceCameraStartTime < 0f
                    ? float.MaxValue
                    : _playerFaceCameraStartTime + PlayerFaceCameraDuration);

            if (!_enemyRunning && _stateTimer >= minRunTime)
            {
                StartEnemyRun();
                _enemyRunning = true;
            }

            if (!_autoRunQueued && _enemyRunning && _stateTimer >= controller.EnemyRunStartDelay + controller.PlayerAutoRunDelay)
            {
                _autoRunQueued = true;
                controller.ChangeState(new JungleChaseAutoRunState(controller));
            }
        }

        void EnsureEnemyReady()
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

        void ConfigureEnemyAudio()
        {
            if (controller.ChaseEnemyController == null) return;

            if (controller.EnemyRoarSound != null)
                controller.ChaseEnemyController.SetRoarClip(controller.EnemyRoarSound);

            if (!controller.EnemyRoarEvent.Equals(default(CorrentesDaNoite.Audio.AudioEvent)))
                controller.ChaseEnemyController.SetRoarEvent(controller.EnemyRoarEvent);
        }

        void TriggerLookBack()
        {
            if (controller.SoundBehindClip != null && controller.AudioSource != null)
                controller.AudioSource.PlayOneShot(controller.SoundBehindClip);

            if (controller.PlayerAnimator != null && !string.IsNullOrEmpty(controller.LookBackAnimationTrigger))
                controller.PlayerAnimator.SetTrigger(controller.LookBackAnimationTrigger);
        }

        void PlaySingleRoar()
        {
            if (_roarPlayed) return;
            _roarPlayed = true;

            if (controller.ChaseEnemyController != null)
                controller.ChaseEnemyController.PlayRoar();

            if (controller.EnableCameraShake)
            {
                var impulse = controller.ChaseEnemy != null
                    ? controller.ChaseEnemy.GetComponent<Unity.Cinemachine.CinemachineImpulseSource>()
                    : null;
                impulse?.GenerateImpulse();
            }
        }

        void StartEnemyRun()
        {
            if (controller.ChaseEnemyController != null)
            {
                controller.ChaseEnemyController.SetTarget(controller.Player);
                controller.ChaseEnemyController.StartChase();
            }

            if (!_chaseCameraActive && controller.GameplayChaseCamera != null)
                controller.SetActiveCamera(controller.GameplayChaseCamera);
        }

        void CutToPlayerFace()
        {
            if (controller.PlayerFaceCamera != null)
                controller.SetActiveCamera(controller.PlayerFaceCamera);
        }
    }
}
