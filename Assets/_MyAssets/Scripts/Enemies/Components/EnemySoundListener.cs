using UnityEngine;
using CorrentesDaNoite.Audio;

namespace CorrentesDaNoite.Enemies
{
    public class EnemySoundListener : MonoBehaviour, IHear
    {
        [Header("Hearing Settings")]
        [SerializeField] protected float hearingRange = 20f;
        [SerializeField] protected bool enableSoundDetection = true;

        [Header("Sound Filtering")]
        [SerializeField] protected bool filterByLayer = true;
        [SerializeField] protected LayerMask soundLayerMask = -1;
        [SerializeField] protected bool ignoreOwnSounds = true;

        [Header("Sound Type Multipliers")]
        [SerializeField] protected float walkingSensitivity = 1f;
        [SerializeField] protected float runningSensitivity = 1f;
        [SerializeField] protected float jumpingSensitivity = 0.8f;
        [SerializeField] protected float landingSensitivity = 1f;

        [Header("Debug")]
        [SerializeField] protected bool showDebugLogs = false;
        [SerializeField] protected bool showDebugGizmos = false;

        protected EnemyController _enemyController;
        protected Vector3 _lastHeardSoundPosition;
        protected SoundType _lastHeardSoundType;
        protected float _lastHeardSoundTime;

        protected virtual void Awake() => _enemyController = GetComponent<EnemyController>();

        public virtual void OnSoundHeard(SoundData sound)
        {
            if (!enableSoundDetection) return;

            if (ignoreOwnSounds && sound.emitter == gameObject)
                return;

            if (filterByLayer && sound.emitter != null)
            {
                int emitterLayer = sound.emitter.layer;
                if (((1 << emitterLayer) & soundLayerMask) == 0)
                    return;
            }

            float distanceToSound = Vector3.Distance(transform.position, sound.position);

            if (distanceToSound > hearingRange)
                return;

            float effectiveRange = sound.range * GetSensitivityMultiplier(sound.soundType);

            if (distanceToSound > effectiveRange)
                return;

            _lastHeardSoundPosition = sound.position;
            _lastHeardSoundType = sound.soundType;
            _lastHeardSoundTime = Time.time;

            if (showDebugLogs)
            {
                Debug.Log($"[{gameObject.name}] Ouviu {sound.soundType} de {sound.emitter?.name ?? "desconhecido"} " +
                          $"a {distanceToSound:F1}m (range efetivo: {effectiveRange:F1}m)");
            }

            if (_enemyController != null)
                _enemyController.OnSoundHeard(sound, distanceToSound);
        }

        protected virtual float GetSensitivityMultiplier(SoundType soundType)
        {
            return soundType switch
            {
                SoundType.Walking => walkingSensitivity,
                SoundType.Running => runningSensitivity,
                SoundType.Jumping => jumpingSensitivity,
                SoundType.Landing => landingSensitivity,
                SoundType.Crouching => 0f,
                SoundType.Struggling => 1f,
                _ => 1f
            };
        }

        public virtual void SetHearingRange(float range) => hearingRange = range;

        public virtual void EnableDetection(bool enable) => enableSoundDetection = enable;

        protected virtual void OnDrawGizmosSelected()
        {
            if (!showDebugGizmos) return;

            Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
            Gizmos.DrawSphere(transform.position, hearingRange);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, hearingRange);

            if (Application.isPlaying && _lastHeardSoundPosition != Vector3.zero)
            {
                float timeSinceHeard = Time.time - _lastHeardSoundTime;
                if (timeSinceHeard < 2f)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(transform.position, _lastHeardSoundPosition);
                    Gizmos.DrawWireSphere(_lastHeardSoundPosition, 0.5f);

                    #if UNITY_EDITOR
                    UnityEditor.Handles.Label(
                        _lastHeardSoundPosition + Vector3.up,
                        $"{_lastHeardSoundType}\n{timeSinceHeard:F1}s ago",
                        new GUIStyle()
                        {
                            normal = new GUIStyleState() { textColor = Color.yellow },
                            fontSize = 12,
                            fontStyle = FontStyle.Bold,
                            alignment = TextAnchor.MiddleCenter
                        }
                    );
                    #endif
                }
            }
        }
    }
}