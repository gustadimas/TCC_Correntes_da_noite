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

        SphereCollider detectionCollider;
        EnemyController enemyController;
        Transform playerTransform;
        bool isDetectingPlayer;

        public bool IsDetectingPlayer => isDetectingPlayer;
        public float LightRadius => lightRadius;

        void Awake()
        {
            enemyController = GetComponent<EnemyController>();

            detectionCollider = GetComponent<SphereCollider>();
            if (detectionCollider == null)
                detectionCollider = gameObject.AddComponent<SphereCollider>();

            detectionCollider.isTrigger = true;
            detectionCollider.radius = lightRadius;

            if (lightSource == null)
                lightSource = transform;
        }

        void Update()
        {
            if (playerTransform != null)
            {
                bool canSeePlayer = !checkObstacles || HasLineOfSight();

                if (canSeePlayer != isDetectingPlayer)
                {
                    isDetectingPlayer = canSeePlayer;

                    if (isDetectingPlayer)
                        enemyController?.OnPlayerDetectedByLight();
                    else
                        enemyController?.OnPlayerLostFromLight();
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            playerTransform = other.transform;
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            playerTransform = null;
            isDetectingPlayer = false;
            enemyController?.OnPlayerLostFromLight();
        }

        bool HasLineOfSight()
        {
            if (playerTransform == null) return false;

            Vector3 direction = playerTransform.position - lightSource.position;
            float distance = direction.magnitude;

            if (Physics.Raycast(lightSource.position, direction.normalized, out RaycastHit hit, distance, obstacleLayer))
            {
                return hit.transform == playerTransform;
            }

            return true;
        }

        public void SetLightRadius(float radius)
        {
            lightRadius = radius;
            if (detectionCollider != null)
                detectionCollider.radius = radius;
        }

        public void EnableDetection(bool enable)
        {
            if (detectionCollider != null)
                detectionCollider.enabled = enable;
        }

        void OnDrawGizmosSelected()
        {
            Vector3 center = lightSource != null ? lightSource.position : transform.position;

            Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
            Gizmos.DrawSphere(center, lightRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center, lightRadius);

            if (playerTransform != null)
            {
                float distance = Vector3.Distance(center, playerTransform.position);

                Gizmos.color = isDetectingPlayer ? Color.green : Color.red;
                Gizmos.DrawLine(center, playerTransform.position);
                Gizmos.DrawWireSphere(playerTransform.position, 0.5f);

                #if UNITY_EDITOR
                UnityEditor.Handles.Label(
                    playerTransform.position + Vector3.up * 2f,
                    $"{distance:F1}m - {(isDetectingPlayer ? "DETECTADO" : "BLOQUEADO")}",
                    new GUIStyle() {
                        normal = new GUIStyleState() { textColor = isDetectingPlayer ? Color.green : Color.red },
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