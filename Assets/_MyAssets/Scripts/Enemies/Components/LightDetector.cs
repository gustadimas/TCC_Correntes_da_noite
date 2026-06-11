using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public class LightDetector : MonoBehaviour
    {
        [Header("Light Settings")]
        [SerializeField] float lightRadius = 15f;
        [SerializeField] Transform lightSource;

        [Header("Detection")]
        [SerializeField] bool checkObstacles = true;
        [SerializeField] LayerMask obstacleLayer;

        SphereCollider _detectionCollider;
        EnemyController _enemyController;
        Transform _playerTransform;
        bool _isDetectingPlayer;
        float _suppressDetectionUntil;

        public bool IsDetectingPlayer => _isDetectingPlayer;
        public float LightRadius => lightRadius;

        void Awake()
        {
            _enemyController = GetComponent<EnemyController>();

            _detectionCollider = GetComponent<SphereCollider>();
            if (_detectionCollider == null)
                _detectionCollider = gameObject.AddComponent<SphereCollider>();

            _detectionCollider.isTrigger = true;
            _detectionCollider.radius = lightRadius;

            if (lightSource == null)
                lightSource = transform;
        }

        void Update()
        {
            if (Time.time < _suppressDetectionUntil)
                return;

            if (_playerTransform != null)
            {
                bool canSeePlayer = !checkObstacles || HasLineOfSight();

                if (canSeePlayer != _isDetectingPlayer)
                {
                    _isDetectingPlayer = canSeePlayer;

                    if (_isDetectingPlayer)
                        _enemyController?.OnPlayerDetectedByLight();
                    else
                        _enemyController?.OnPlayerLostFromLight();
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerTransform = other.transform;
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            _playerTransform = null;
            _isDetectingPlayer = false;
            _enemyController?.OnPlayerLostFromLight();
        }

        bool HasLineOfSight()
        {
            if (_playerTransform == null) return false;

            Vector3 direction = _playerTransform.position - lightSource.position;
            float distance = direction.magnitude;

            if (Physics.Raycast(lightSource.position, direction.normalized, out RaycastHit hit, distance, obstacleLayer))
            {
                return hit.transform == _playerTransform;
            }

            return true;
        }

        public void SetLightRadius(float radius)
        {
            lightRadius = radius;
            if (_detectionCollider != null)
                _detectionCollider.radius = radius;
        }

        public void EnableDetection(bool enable)
        {
            if (_detectionCollider != null)
                _detectionCollider.enabled = enable;
        }

        public void ResetDetectionState(float suppressDuration = 0f)
        {
            _playerTransform = null;
            _isDetectingPlayer = false;
            _suppressDetectionUntil = suppressDuration > 0f ? Time.time + suppressDuration : 0f;
        }

        void OnDrawGizmosSelected()
        {
            Vector3 center = lightSource != null ? lightSource.position : transform.position;

            Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
            Gizmos.DrawSphere(center, lightRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center, lightRadius);

            if (_playerTransform != null)
            {
                float distance = Vector3.Distance(center, _playerTransform.position);

                Gizmos.color = _isDetectingPlayer ? Color.green : Color.red;
                Gizmos.DrawLine(center, _playerTransform.position);
                Gizmos.DrawWireSphere(_playerTransform.position, 0.5f);

                #if UNITY_EDITOR
                UnityEditor.Handles.Label(
                    _playerTransform.position + Vector3.up * 2f,
                    $"{distance:F1}m - {(_isDetectingPlayer ? "DETECTADO" : "BLOQUEADO")}",
                    new GUIStyle()
                    {
                        normal = new GUIStyleState() { textColor = _isDetectingPlayer ? Color.green : Color.red },
                        fontSize = 14,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter
                    }
                );
                #endif
            }
        }
    }
}