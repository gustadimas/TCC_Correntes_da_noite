using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public class EnemyVisionDetector : MonoBehaviour
    {
        [Header("Vision Settings")]
        [SerializeField] protected float viewDistance = 12f;
        [SerializeField, Range(0f, 360f)] protected float viewAngle = 90f;
        [SerializeField] protected float timeToAggro = 0.75f;
        [SerializeField] protected LayerMask obstructionMask = ~0;
        [SerializeField] protected Transform eyeOrigin;

        [Header("Update")]
        [SerializeField] protected bool detectionEnabled = true;
        [SerializeField] protected float visibilityResetDelay = 0.1f;

        protected EnemyController enemyController;
        protected Transform playerTransform;
        protected float seeingTimer;
        protected float lossTimer;
        protected bool canSeePlayer;

        protected virtual void Awake()
        {
            enemyController = GetComponent<EnemyController>();
            if (eyeOrigin == null)
                eyeOrigin = transform;
        }

        protected virtual void OnEnable()
        {
            seeingTimer = 0f;
            lossTimer = 0f;
            canSeePlayer = false;
        }

        protected virtual void Update()
        {
            if (!detectionEnabled)
                return;

            EnsurePlayerReference();
            if (playerTransform == null || enemyController == null)
                return;

            bool isVisible = IsPlayerVisible();

            if (isVisible)
            {
                lossTimer = 0f;
                seeingTimer += Time.deltaTime;

                if (seeingTimer >= timeToAggro)
                {
                    enemyController.OnVisionAggro();
                    seeingTimer = 0f;
                }
            }
            else
            {
                lossTimer += Time.deltaTime;
                if (lossTimer >= visibilityResetDelay)
                {
                    lossTimer = 0f;
                    seeingTimer = 0f;
                }
            }

            canSeePlayer = isVisible;
        }

        protected virtual bool IsPlayerVisible()
        {
            Vector3 origin = eyeOrigin != null ? eyeOrigin.position : transform.position;
            Vector3 toPlayer = playerTransform.position - origin;

            if (toPlayer.sqrMagnitude > viewDistance * viewDistance)
                return false;

            Vector3 flatToPlayer = toPlayer;
            flatToPlayer.y = 0f;

            Vector3 forward = eyeOrigin != null ? eyeOrigin.forward : transform.forward;
            forward.y = 0f;

            if (flatToPlayer.sqrMagnitude < 0.0001f || forward.sqrMagnitude < 0.0001f)
                return true;

            float halfAngle = viewAngle * 0.5f;
            float angleToPlayer = Vector3.Angle(forward, flatToPlayer);
            if (angleToPlayer > halfAngle)
                return false;

            float distance = Mathf.Sqrt(toPlayer.sqrMagnitude);
            if (Physics.Raycast(
                    origin,
                    toPlayer.normalized,
                    out RaycastHit hit,
                    distance,
                    obstructionMask,
                    QueryTriggerInteraction.Ignore))
            {
                bool hitSelf = hit.transform == transform || hit.transform.IsChildOf(transform);
                if (hitSelf)
                    return true;

                return hit.transform == playerTransform || hit.transform.IsChildOf(playerTransform);
            }

            return true;
        }

        protected virtual void EnsurePlayerReference()
        {
            if (playerTransform != null)
                return;

            if (enemyController != null && enemyController.PlayerTransform != null)
            {
                playerTransform = enemyController.PlayerTransform;
                return;
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }

        public virtual void EnableDetection(bool enable)
        {
            detectionEnabled = enable;
            if (!enable)
            {
                seeingTimer = 0f;
                lossTimer = 0f;
                canSeePlayer = false;
            }
        }

        protected virtual void OnDrawGizmosSelected()
        {
            if (!detectionEnabled)
                return;

            Vector3 origin = eyeOrigin != null ? eyeOrigin.position : transform.position;
            Vector3 forward = eyeOrigin != null ? eyeOrigin.forward : transform.forward;

            Gizmos.color = new Color(0f, 0.5f, 1f, 0.15f);
            Gizmos.DrawWireSphere(origin, viewDistance);

            Vector3 leftBoundary = Quaternion.Euler(0f, -viewAngle * 0.5f, 0f) * forward;
            Vector3 rightBoundary = Quaternion.Euler(0f, viewAngle * 0.5f, 0f) * forward;

            Gizmos.color = canSeePlayer ? Color.green : Color.cyan;
            Gizmos.DrawLine(origin, origin + leftBoundary.normalized * viewDistance);
            Gizmos.DrawLine(origin, origin + rightBoundary.normalized * viewDistance);
        }
    }
}
