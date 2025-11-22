using UnityEngine;

namespace CorrentesDaNoite.Chase
{
    public class ChasePathFollower : MonoBehaviour
    {
        [Header("Path Settings")]
        [SerializeField] Transform[] waypoints;
        [SerializeField] bool loopPath;
        [SerializeField] float waypointReachedThreshold = 1f;

        [Header("Auto Movement")]
        [SerializeField] bool autoMove;
        [SerializeField] float autoMoveSpeed = 6f;

        [Header("State")]
        [SerializeField] int currentWaypointIndex;
        [SerializeField] bool pathCompleted;

        [Header("Debug")]
        [SerializeField] bool debugMode;
        [SerializeField] Color pathColor = Color.green;

        public Transform[] Waypoints => waypoints;
        public int CurrentWaypointIndex => currentWaypointIndex;
        public bool PathCompleted => pathCompleted;
        public Transform CurrentWaypoint => GetCurrentWaypoint();
        public Transform NextWaypoint => GetNextWaypoint();

        void Update()
        {
            if (autoMove && !pathCompleted)
                MoveTowardsCurrentWaypoint();
        }

        void MoveTowardsCurrentWaypoint()
        {
            Transform currentWaypoint = GetCurrentWaypoint();
            if (currentWaypoint == null)
            {
                pathCompleted = true;
                return;
            }

            Vector3 targetPosition = currentWaypoint.position;
            Vector3 direction = (targetPosition - transform.position);
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.0001f)
            {
                OnWaypointReached();
                return;
            }

            Vector3 step = direction.normalized * autoMoveSpeed * Time.deltaTime;
            transform.position += step;

            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }

            if (Vector3.Distance(transform.position, targetPosition) <= waypointReachedThreshold)
                OnWaypointReached();
        }

        void OnWaypointReached()
        {
            if (debugMode)
                Debug.Log($"[ChasePathFollower] Waypoint {currentWaypointIndex} alcancado");

            currentWaypointIndex++;

            if (waypoints == null || waypoints.Length == 0)
            {
                pathCompleted = true;
                return;
            }

            if (currentWaypointIndex >= waypoints.Length)
            {
                if (loopPath)
                {
                    currentWaypointIndex = 0;
                    if (debugMode)
                        Debug.Log("[ChasePathFollower] Loop ativado, voltando ao inicio");
                }
                else
                {
                    pathCompleted = true;
                    if (debugMode)
                        Debug.Log("[ChasePathFollower] Caminho completo");
                }
            }
        }

        public Transform GetCurrentWaypoint()
        {
            if (waypoints == null || waypoints.Length == 0)
                return null;

            if (currentWaypointIndex < 0 || currentWaypointIndex >= waypoints.Length)
                return null;

            return waypoints[currentWaypointIndex];
        }

        public Transform GetNextWaypoint()
        {
            if (waypoints == null || waypoints.Length == 0)
                return null;

            int nextIndex = currentWaypointIndex + 1;

            if (nextIndex >= waypoints.Length)
            {
                if (loopPath)
                    nextIndex = 0;
                else
                    return null;
            }

            return waypoints[nextIndex];
        }

        public Vector3 GetDirectionToCurrentWaypoint()
        {
            Transform current = GetCurrentWaypoint();
            if (current == null)
                return Vector3.forward;

            Vector3 direction = (current.position - transform.position).normalized;
            direction.y = 0f;
            return direction;
        }

        public float GetDistanceToCurrentWaypoint()
        {
            Transform current = GetCurrentWaypoint();
            if (current == null)
                return 0f;

            return Vector3.Distance(transform.position, current.position);
        }

        public void ResetPath()
        {
            currentWaypointIndex = 0;
            pathCompleted = false;

            if (waypoints != null && waypoints.Length > 0 && waypoints[0] != null)
                transform.position = waypoints[0].position;

            if (debugMode)
                Debug.Log("[ChasePathFollower] Caminho resetado");
        }

        public void SetAutoMove(bool enabled) => autoMove = enabled;

        public void SetAutoMoveSpeed(float speed) => autoMoveSpeed = speed;

        public void GoToWaypoint(int index)
        {
            if (waypoints == null || waypoints.Length == 0) return;
            if (index >= 0 && index < waypoints.Length)
            {
                currentWaypointIndex = index;
                pathCompleted = false;
            }
        }

        void OnDrawGizmos()
        {
            if (waypoints == null || waypoints.Length == 0)
                return;

            Gizmos.color = pathColor;

            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null)
                    continue;

                Gizmos.DrawWireSphere(waypoints[i].position, 0.5f);

                if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
                else if (loopPath && waypoints[0] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
                }
            }

            if (Application.isPlaying && GetCurrentWaypoint() != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, GetCurrentWaypoint().position);
                Gizmos.DrawWireSphere(GetCurrentWaypoint().position, 1f);
            }
        }

        void OnDrawGizmosSelected()
        {
            if (waypoints == null || waypoints.Length == 0)
                return;

            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null)
                    continue;

                Vector3 labelPos = waypoints[i].position + Vector3.up * 2f;

#if UNITY_EDITOR
                UnityEditor.Handles.Label(
                    labelPos,
                    $"Waypoint {i}",
                    new GUIStyle()
                    {
                        normal = { textColor = pathColor },
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
