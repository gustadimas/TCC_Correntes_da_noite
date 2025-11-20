using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public class EnemySphereDetection : EnemyDetectionBase
    {
        [Header("Sphere Detection Settings")]
        [SerializeField] protected SphereCollider detectionSphere;
        [SerializeField] protected bool checkLineOfSight = true;
        [SerializeField] protected Transform detectionPoint;

        protected Transform playerInSphere;
        protected bool hasDetected;

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

            if (playerInSphere != null)
            {
                bool canSee = !checkLineOfSight || CheckLineOfSightToPlayer();

                if (canSee && !hasDetected)
                {
                    hasDetected = true;
                    detectedTarget = playerInSphere;
                }
                else if (!canSee && hasDetected)
                {
                    hasDetected = false;
                    detectedTarget = null;
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!detectionEnabled) return;

            if (IsPlayer(other))
            {
                playerInSphere = other.transform;

                if (!checkLineOfSight)
                {
                    hasDetected = true;
                    detectedTarget = playerInSphere;
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (!detectionEnabled) return;

            if (IsPlayer(other))
            {
                playerInSphere = null;
                hasDetected = false;
                detectedTarget = null;
            }
        }

        protected virtual bool CheckLineOfSightToPlayer()
        {
            if (playerInSphere == null) return false;

            Vector3 directionToPlayer = playerInSphere.position - detectionPoint.position;
            float distanceToPlayer = directionToPlayer.magnitude;

            if (Physics.Raycast(detectionPoint.position, directionToPlayer.normalized, out RaycastHit hit, distanceToPlayer, obstacleLayer))
            {
                if (hit.transform != playerInSphere)
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
            return hasDetected && detectedTarget != null;
        }

        public void SetDetectionRadius(float radius)
        {
            detectionRadius = radius;
            if (detectionSphere != null)
                detectionSphere.radius = radius;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = hasDetected ? Color.red : Color.cyan;
            Vector3 center = detectionPoint != null ? detectionPoint.position : transform.position;
            Gizmos.DrawWireSphere(center, detectionRadius);

            if (playerInSphere != null)
            {
                Gizmos.color = hasDetected ? Color.green : Color.yellow;
                Gizmos.DrawLine(center, playerInSphere.position);
            }
        }
    }
}