using UnityEngine;
using System.Collections;
using CorrentesDaNoite.Player;
using CorrentesDaNoite.Checkpoint;

namespace CorrentesDaNoite.Enemies
{
    [RequireComponent(typeof(Collider))]
    public class WatchGuardLight : MonoBehaviour
    {
        [Header("Light Setup")]
        [SerializeField] protected Light spotLight;
        [SerializeField] protected Transform origin;

        [Header("Detection")]
        [SerializeField] protected float viewRange = 8f;
        [SerializeField] protected float viewAngle = 45f;
        [SerializeField] protected LayerMask obstacleLayer;
        [SerializeField] protected bool checkLineOfSight = true;
        [SerializeField] protected bool useConeCheck = true;
        [SerializeField] protected bool killOnEnter = true;
        [SerializeField] protected string playerTag = "Player";
        [SerializeField] protected string playerReactionTrigger = "";
        [SerializeField] protected float killDelay = 0.2f;

        [Header("Debug")]
        [SerializeField] protected bool drawGizmos = false;

        protected Collider _trigger;
        protected bool _isKilling;
        protected bool _isActive = true;

        void Awake()
        {
            _trigger = GetComponent<Collider>();
            _trigger.isTrigger = true;

            if (spotLight == null)
                spotLight = GetComponentInChildren<Light>();

            if (origin == null)
                origin = transform;

            SyncLightShape();
        }

        void OnValidate()
        {
            SyncLightShape();
        }

        void SyncLightShape()
        {
            if (spotLight != null)
            {
                spotLight.spotAngle = viewAngle * 2f;
                spotLight.range = viewRange;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            EvaluateDetection(other);
        }

        void OnTriggerStay(Collider other)
        {
            EvaluateDetection(other);
        }

        public void SetLightActive(bool enabled)
        {
            _isActive = enabled;

            if (spotLight != null)
            {
                spotLight.enabled = enabled;
                spotLight.gameObject.SetActive(enabled);
            }
        }

        void EvaluateDetection(Collider other)
        {
            if (!IsPlayer(other))
                return;

            if (!_isActive)
                return;

            Vector3 targetPosition = other.transform.position;
            if (useConeCheck && !IsInsideCone(targetPosition))
                return;

            if (checkLineOfSight && !HasLineOfSight(targetPosition, other))
                return;

            if (!_isKilling)
                StartCoroutine(KillRoutine(other.gameObject));
        }

        IEnumerator KillRoutine(GameObject player)
        {
            _isKilling = true;

            if (killDelay > 0f)
                yield return new WaitForSeconds(killDelay);

            KillPlayer(player);
            _isKilling = false;
        }

        bool IsPlayer(Collider other)
        {
            return other.CompareTag(playerTag);
        }

        bool IsInsideCone(Vector3 targetPosition)
        {
            Vector3 originPosition = origin != null ? origin.position : transform.position;
            Vector3 toTarget = targetPosition - originPosition;
            float distance = toTarget.magnitude;

            if (distance > viewRange)
                return false;

            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.0001f)
                return true;

            float angleToTarget = Vector3.Angle(origin.forward, toTarget);
            return angleToTarget <= viewAngle;
        }

        bool HasLineOfSight(Vector3 targetPosition, Collider targetCollider)
        {
            Vector3 originPosition = origin != null ? origin.position : transform.position;
            Vector3 direction = (targetPosition - originPosition).normalized;
            float distance = Vector3.Distance(originPosition, targetPosition);

            if (Physics.Raycast(originPosition, direction, out RaycastHit hit, distance, obstacleLayer))
            {
                return hit.collider == targetCollider || hit.transform == targetCollider.transform;
            }

            return true;
        }

        void KillPlayer(GameObject player)
        {
            if (!killOnEnter)
                return;

            CheckpointManager manager = CheckpointManager.Instance;
            if (manager != null)
            {
                manager.RespawnPlayer(player, false, playerReactionTrigger);
                return;
            }

            PlayerDeath playerDeath = player.GetComponent<PlayerDeath>();
            if (playerDeath != null)
            {
                playerDeath.DieFromLight();
                return;
            }
        }

        void OnDisable()
        {
            _isKilling = false;
        }

        void OnDrawGizmosSelected()
        {
            if (!drawGizmos)
                return;

            Vector3 originPosition = origin != null ? origin.position : transform.position;
            Vector3 forward = origin != null ? origin.forward : transform.forward;

            Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
            DrawSolidConeGizmo(originPosition, forward, viewRange, viewAngle);

            Gizmos.color = Color.yellow;
            DrawWireConeGizmo(originPosition, forward, viewRange, viewAngle);
        }

        void DrawWireConeGizmo(Vector3 originPosition, Vector3 forward, float range, float angle)
        {
            Quaternion leftRot = Quaternion.AngleAxis(-angle, Vector3.up);
            Quaternion rightRot = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 leftDir = leftRot * forward;
            Vector3 rightDir = rightRot * forward;

            Vector3 tip = originPosition;
            Vector3 farCenter = originPosition + forward.normalized * range;

            Gizmos.DrawLine(tip, tip + leftDir.normalized * range);
            Gizmos.DrawLine(tip, tip + rightDir.normalized * range);
            Gizmos.DrawWireSphere(farCenter, 0.05f * range);
        }

        void DrawSolidConeGizmo(Vector3 originPosition, Vector3 forward, float range, float angle)
        {
            int segments = 24;
            Vector3[] arcPoints = new Vector3[segments + 1];
            Quaternion rotStep = Quaternion.AngleAxis((angle * 2f) / segments, Vector3.up);
            Vector3 currentDir = Quaternion.AngleAxis(-angle, Vector3.up) * forward.normalized;

            for (int i = 0; i <= segments; i++)
            {
                arcPoints[i] = originPosition + currentDir * range;
                currentDir = rotStep * currentDir;
            }

            for (int i = 0; i < segments; i++)
            {
                Gizmos.DrawLine(originPosition, arcPoints[i]);
                Gizmos.DrawLine(arcPoints[i], arcPoints[i + 1]);
            }
        }
    }
}