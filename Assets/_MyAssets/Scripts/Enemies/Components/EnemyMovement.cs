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

        NavMeshAgent _navMeshAgent;
        Rigidbody _rigidbody;
        EnemyController _controller;
        bool _lastMoveSucceeded;

        public float MoveSpeed => moveSpeed;
        public bool IsMoving => _navMeshAgent != null && _navMeshAgent.hasPath;
        public bool LastMoveSucceeded => _lastMoveSucceeded;
        public bool HasValidPath
        {
            get
            {
                if (_navMeshAgent == null)
                    return false;
                if (_navMeshAgent.pathPending)
                    return true;
                if (!_navMeshAgent.hasPath)
                    return false;
                return _navMeshAgent.pathStatus != NavMeshPathStatus.PathInvalid;
            }
        }

        void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _rigidbody = GetComponent<Rigidbody>();
            _controller = GetComponent<EnemyController>() ?? GetComponentInParent<EnemyController>();

            if (_navMeshAgent == null)
            {
                Debug.LogError($"[EnemyMovement] NavMeshAgent not found on {gameObject.name}!");
                return;
            }

            _navMeshAgent.acceleration = acceleration;
            _navMeshAgent.angularSpeed = angularSpeed;
            _navMeshAgent.stoppingDistance = stoppingDistance;
            _navMeshAgent.updateRotation = false;

            moveSpeed = _navMeshAgent.speed;

            if (_rigidbody != null)
                _rigidbody.isKinematic = true;

            TrySnapAgentToNavMesh(transform.position, 5f);
        }

        void Update()
        {
            if (_navMeshAgent == null) return;

            if (_controller != null &&
                (_controller.IsRotatingToSound || _controller.IsRotatingBackToPatrol))
                return;

            if (!_navMeshAgent.hasPath) return;

            Vector3 direction = _navMeshAgent.desiredVelocity;
            if (direction.sqrMagnitude < 0.01f)
                direction = _navMeshAgent.steeringTarget - transform.position;

            if (direction.sqrMagnitude > 0.0001f)
            {
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

        public bool MoveTo(Vector3 target)
        {
            if (_navMeshAgent == null)
            {
                _lastMoveSucceeded = false;
                return false;
            }

            if (!_navMeshAgent.isOnNavMesh && !TrySnapAgentToNavMesh(transform.position))
            {
                _lastMoveSucceeded = false;
                return false;
            }

            if (!NavMesh.SamplePosition(target, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                Debug.LogWarning($"[EnemyMovement] Failed to find NavMesh near target for {gameObject.name}.");
                _lastMoveSucceeded = false;
                return false;
            }

            _navMeshAgent.SetDestination(hit.position);
            _navMeshAgent.isStopped = false;
            _lastMoveSucceeded = true;
            return true;
        }

        public void Stop()
        {
            if (_navMeshAgent == null) return;

            _navMeshAgent.isStopped = true;
            _navMeshAgent.ResetPath();
            _lastMoveSucceeded = false;
        }

        public void TeleportTo(Vector3 position)
        {
            if (_navMeshAgent == null)
            {
                transform.position = position;
                _lastMoveSucceeded = false;
                return;
            }

            if (!TrySnapAgentToNavMesh(position))
            {
                _lastMoveSucceeded = false;
                return;
            }

            _navMeshAgent.ResetPath();
            _lastMoveSucceeded = true;
        }

        public void SetSpeed(float speed)
        {
            moveSpeed = speed;

            if (_navMeshAgent != null)
                _navMeshAgent.speed = speed;
        }

        public bool HasReachedDestination(float threshold = 0.5f)
        {
            if (_navMeshAgent == null) return false;
            if (_navMeshAgent.pathPending) return false;
            if (!_navMeshAgent.hasPath) return false;
            if (_navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid) return false;

            float checkDistance = Mathf.Max(_navMeshAgent.stoppingDistance, threshold);
            return _navMeshAgent.remainingDistance <= checkDistance;
        }

        public void SetAcceleration(float accel)
        {
            acceleration = accel;

            if (_navMeshAgent != null)
                _navMeshAgent.acceleration = accel;
        }

        bool TrySnapAgentToNavMesh(Vector3 desiredPosition, float maxDistance = 2f)
        {
            if (_navMeshAgent == null)
                return false;

            if (NavMesh.SamplePosition(desiredPosition, out NavMeshHit hit, maxDistance, NavMesh.AllAreas))
            {
                _navMeshAgent.Warp(hit.position);
                return true;
            }

            Debug.LogWarning($"[EnemyMovement] No NavMesh found near {desiredPosition} for {gameObject.name}.");
            return false;
        }
    }
}