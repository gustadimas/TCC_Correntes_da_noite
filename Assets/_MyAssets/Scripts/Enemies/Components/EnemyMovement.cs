using UnityEngine;
using UnityEngine.AI;

namespace CorrentesDaNoite.Enemies
{
    public class EnemyMovement : MonoBehaviour
    {
        [Header("Speed")]
        [SerializeField] float moveSpeed = 3f;
        [SerializeField] float rotationSpeed = 360f;

        [Header("NavMesh Settings")]
        [SerializeField] float acceleration = 16f;
        [SerializeField] float angularSpeed = 360f;
        [SerializeField] float stoppingDistance = 0.2f;

        NavMeshAgent navMeshAgent;
        Rigidbody rb;

        public float MoveSpeed => moveSpeed;
        public bool IsMoving => navMeshAgent != null && navMeshAgent.hasPath;

        void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            rb = GetComponent<Rigidbody>();

            if (navMeshAgent == null)
            {
                Debug.LogError($"[EnemyMovement] NavMeshAgent não encontrado em {gameObject.name}!");
                return;
            }

            navMeshAgent.acceleration = acceleration;
            navMeshAgent.angularSpeed = angularSpeed;
            navMeshAgent.stoppingDistance = stoppingDistance;
            navMeshAgent.updateRotation = false;

            moveSpeed = navMeshAgent.speed;

            if (rb != null)
                rb.isKinematic = true;
        }

        void Update()
        {
            if (navMeshAgent == null || !navMeshAgent.hasPath) return;

            if (navMeshAgent.velocity.sqrMagnitude > 0.01f)
            {
                Vector3 direction = navMeshAgent.velocity;
                direction.y = 0f;

                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.RotateTowards(
                        transform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime
                    );
                }
            }
        }

        public void MoveTo(Vector3 target)
        {
            if (navMeshAgent == null || !navMeshAgent.isOnNavMesh) return;

            navMeshAgent.SetDestination(target);
            navMeshAgent.isStopped = false;
        }

        public void Stop()
        {
            if (navMeshAgent == null) return;

            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();
        }

        public void TeleportTo(Vector3 position)
        {
            if (navMeshAgent != null)
            {
                navMeshAgent.Warp(position);
                navMeshAgent.ResetPath();
            }
            else
                transform.position = position;
        }

        public void SetSpeed(float speed)
        {
            moveSpeed = speed;

            if (navMeshAgent != null)
                navMeshAgent.speed = speed;
        }

        public bool HasReachedDestination(float threshold = 0.5f)
        {
            if (navMeshAgent == null) return false;
            if (navMeshAgent.pathPending) return false;
            if (!navMeshAgent.hasPath) return false;
            if (navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid) return false;

            float checkDistance = Mathf.Max(navMeshAgent.stoppingDistance, threshold);
            return navMeshAgent.remainingDistance <= checkDistance;
        }

        public void SetAcceleration(float accel)
        {
            acceleration = accel;

            if (navMeshAgent != null)
                navMeshAgent.acceleration = accel;
        }
    }
}