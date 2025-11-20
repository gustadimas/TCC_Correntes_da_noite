using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public class EnemySphereDetection : EnemyDetectionBase
    {
        [Header("Sphere Detection Settings")]
        [SerializeField] protected SphereCollider detectionSphere;
        [SerializeField] protected bool checkLineOfSight = true;
        [SerializeField] protected Transform detectionPoint;

        protected Transform _playerInSphere;
        protected bool _hasDetected;

        protected override void Awake()
        {
            base.Awake();

            if (detectionSphere == null)
            {
                detectionSphere = GetComponent<SphereCollider>();
                if (detectionSphere == null)
                    detectionSphere = gameObject.AddComponent<SphereCollider>();
            }

            detectionSphere.isTrigger = true;
            detectionSphere.radius = detectionRadius;

            if (detectionPoint == null)
                detectionPoint = transform;
        }

        protected virtual void Update()
        {
            if (!detectionEnabled) return;

            if (_playerInSphere != null)
            {
                bool canSee = !checkLineOfSight || CheckLineOfSightToPlayer();

                if (canSee && !_hasDetected)
                {
                    _hasDetected = true;
                    _detectedTarget = _playerInSphere;
                }
                else if (!canSee && _hasDetected)
                {
                    _hasDetected = false;
                    _detectedTarget = null;
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!detectionEnabled) return;

            if (IsPlayer(other))
            {
                _playerInSphere = other.transform;

                if (!checkLineOfSight)
                {
                    _hasDetected = true;
                    _detectedTarget = _playerInSphere;
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (!detectionEnabled) return;

            if (IsPlayer(other))
            {
                _playerInSphere = null;
                _hasDetected = false;
                _detectedTarget = null;
            }
        }

        protected virtual bool CheckLineOfSightToPlayer()
        {
            if (_playerInSphere == null) return false;

            Vector3 directionToPlayer = _playerInSphere.position - detectionPoint.position;
            float distanceToPlayer = directionToPlayer.magnitude;

            if (Physics.Raycast(detectionPoint.position, directionToPlayer.normalized, out RaycastHit hit, distanceToPlayer, obstacleLayer))
            {
                if (hit.transform != _playerInSphere)
                {
                    return false;
                }
            }

            return true;
        }

        protected virtual bool IsPlayer(Collider other)
        {
            return other.CompareTag("Player") || ((1 << other.gameObject.layer) & targetLayer) != 0;
        }

        public override void EnableDetection(bool enable)
        {
            detectionEnabled = enable;
            if (detectionSphere != null)
                detectionSphere.enabled = enable;
        }

        public override bool CheckForTarget()
        {
            return _hasDetected && _detectedTarget != null;
        }

        public void SetDetectionRadius(float radius)
        {
            detectionRadius = radius;
            if (detectionSphere != null)
                detectionSphere.radius = radius;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = _hasDetected ? Color.red : Color.cyan;
            Vector3 center = detectionPoint != null ? detectionPoint.position : transform.position;
            Gizmos.DrawWireSphere(center, detectionRadius);

            if (_playerInSphere != null)
            {
                Gizmos.color = _hasDetected ? Color.green : Color.yellow;
                Gizmos.DrawLine(center, _playerInSphere.position);
            }
        }
    }
}