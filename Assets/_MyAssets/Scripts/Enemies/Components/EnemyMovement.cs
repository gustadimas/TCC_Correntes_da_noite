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

        public float MoveSpeed => moveSpeed;
        public bool IsMoving => _navMeshAgent != null && _navMeshAgent.hasPath;

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

        public void MoveTo(Vector3 target)
        {
            if (_navMeshAgent == null || !_navMeshAgent.isOnNavMesh) return;

            _navMeshAgent.SetDestination(target);
            _navMeshAgent.isStopped = false;
        }

        public void Stop()
        {
            if (_navMeshAgent == null) return;

            _navMeshAgent.isStopped = true;
            _navMeshAgent.ResetPath();
        }

        public void TeleportTo(Vector3 position)
        {
            if (_navMeshAgent != null)
            {
                _navMeshAgent.Warp(position);
                _navMeshAgent.ResetPath();
            }
            else
                transform.position = position;
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
    }
}
