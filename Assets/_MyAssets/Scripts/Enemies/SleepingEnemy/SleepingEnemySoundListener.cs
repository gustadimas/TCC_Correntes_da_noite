using UnityEngine;
using CorrentesDaNoite.Audio;

namespace CorrentesDaNoite.Enemies
{
    public class SleepingEnemySoundListener : EnemySoundListener
    {
        [Header("Sleeping Hearing")]
        [SerializeField] protected float extraHearingRadius = 2f;
        [SerializeField] protected float sensitivityMultiplier = 1.2f;
        [SerializeField] protected float startledSilenceTime = 1f;

        protected float _lastHeardTime = -999f;

        protected SleepingEnemyController SleepingController => _enemyController as SleepingEnemyController;

        protected void Update()
        {
            if (SleepingController == null || SleepingController.StateMachine == null) return;

            if (SleepingController.StateMachine.CurrentState is SleepingEnemyStartledState)
            {
                if (Time.time - _lastHeardTime >= startledSilenceTime)
                    SleepingController.GoToSleeping();
            }
        }

        public override void OnSoundHeard(SoundData sound)
        {
            if (!enableSoundDetection) return;
            if (SleepingController == null || SleepingController.StateMachine == null) return;
            if (ignoreOwnSounds && sound.emitter == gameObject) return;

            if (filterByLayer && sound.emitter != null)
            {
                int emitterLayer = sound.emitter.layer;
                if (((1 << emitterLayer) & soundLayerMask) == 0)
                    return;
            }

            float distanceToSound = Vector3.Distance(transform.position, sound.position);
            float baseRange = hearingRange + extraHearingRadius;

            if (distanceToSound > baseRange)
                return;

            float effectiveRange = (sound.range + extraHearingRadius) * sensitivityMultiplier * GetSensitivityMultiplier(sound.soundType);
            if (distanceToSound > effectiveRange)
                return;

            _lastHeardTime = Time.time;

            var currentState = SleepingController.StateMachine.CurrentState;

            if (currentState is EnemyChaseState or EnemyCaptureState)
                return;

            if (currentState is SleepingEnemySleepingState)
            {
                SleepingController.GoToStartled();
                return;
            }

            if (currentState is SleepingEnemyStartledState)
                SleepingController.GoToIdleReady();
        }
    }
}