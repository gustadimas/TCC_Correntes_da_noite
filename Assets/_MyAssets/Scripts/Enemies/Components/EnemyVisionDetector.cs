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

        protected EnemyController enemyController;
        protected Transform playerTransform;
        protected float seeingTimer;
        protected bool canSeePlayer;
        float _suppressDetectionUntil;

        const float VISION_DECAY = 0.5f;

        public bool IsNoticingPlayer => seeingTimer > 0f;

        protected virtual void Awake()
        {
            enemyController = GetComponent<EnemyController>();
            if (eyeOrigin == null)
                eyeOrigin = transform;
        }

        protected virtual void OnEnable()
        {
            seeingTimer = 0f;
            canSeePlayer = false;
        }

        protected virtual void Update()
        {
            if (!detectionEnabled || Time.time < _suppressDetectionUntil) return;

            EnsurePlayerReference();
            if (playerTransform == null || enemyController == null) return;

            canSeePlayer = IsPlayerVisible();
            if (!canSeePlayer)
            {
                seeingTimer = Mathf.Max(0f, seeingTimer - Time.deltaTime * VISION_DECAY);
                return;
            }

            seeingTimer += Time.deltaTime * GetSpotSpeed();
            if (seeingTimer < timeToAggro) return;

            enemyController.OnVisionAggro();
            seeingTimer = 0f;
        }

        // perto ve quase na hora e no limite da visao demora mais
        float GetSpotSpeed()
        {
            float speed = Mathf.Lerp(2.5f, 0.6f, Vector3.Distance(eyeOrigin.position, playerTransform.position) / viewDistance);
            var player = enemyController.Player;
            if (player == null) return speed;
            if (player.IsCrouching) return speed * 0.5f;
            return player.IsRunning ? speed * 2f : speed;
        }

        // mira no peito pra um caixote baixo nao esconder ela inteira
        Vector3 GetAimPoint()
        {
            bool crouching = enemyController.Player != null && enemyController.Player.IsCrouching;
            return playerTransform.position + Vector3.up * (crouching ? 0.6f : 1.3f);
        }

        protected virtual bool IsPlayerVisible()
        {
            Vector3 origin = eyeOrigin != null ? eyeOrigin.position : transform.position;
            Vector3 toPlayer = GetAimPoint() - origin;

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
                canSeePlayer = false;
            }
        }

        public virtual void ResetDetectionState(float suppressDuration = 0f)
        {
            playerTransform = null;
            seeingTimer = 0f;
            canSeePlayer = false;
            _suppressDetectionUntil = suppressDuration > 0f ? Time.time + suppressDuration : 0f;
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