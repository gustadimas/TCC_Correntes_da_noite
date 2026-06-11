using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public class WatchGuardController : EnemyController
    {
        [Header("Config")]
        [SerializeField] protected WatchGuardConfig watchGuardConfig;

        [Header("Watch Guard Settings")]
        [SerializeField] protected Transform[] guardPoints;
        [SerializeField] protected Transform[] lookTargets;
        [SerializeField] protected float guardMoveSpeed = 2f;
        [SerializeField] protected float arrivalThreshold = 0.4f;
        [SerializeField] protected float lookRotationSpeed = 180f;
        [SerializeField] protected float lookDuration = 2f;
        [SerializeField] protected float lookAlignmentTolerance = 2f;
        [SerializeField] protected bool loopGuardPoints = true;

        protected WatchGuardLight _guardLight;

        public Transform[] GuardPoints => guardPoints;
        public Transform[] LookTargets => lookTargets;
        public float GuardMoveSpeed => guardMoveSpeed;
        public float ArrivalThreshold => arrivalThreshold;
        public float LookRotationSpeed => lookRotationSpeed;
        public float LookDuration => lookDuration;
        public float LookAlignmentTolerance => lookAlignmentTolerance;
        public bool LoopGuardPoints => loopGuardPoints;
        public WatchGuardAnimationController WatchAnimation => animationController as WatchGuardAnimationController;
        public WatchGuardLight GuardLight => _guardLight;

        protected override void ApplyConfig()
        {
            base.ApplyConfig();

            enableSoundReactions = false;

            if (watchGuardConfig == null)
                return;

            guardMoveSpeed = watchGuardConfig.guardMoveSpeed;
            arrivalThreshold = watchGuardConfig.guardArrivalThreshold;
            lookRotationSpeed = watchGuardConfig.guardLookRotationSpeed;
            lookDuration = watchGuardConfig.guardLookDuration;
            lookAlignmentTolerance = watchGuardConfig.guardLookAlignmentTolerance;
            loopGuardPoints = watchGuardConfig.guardLoopPoints;
        }

        protected override void InitializeStateMachine()
        {
            if (HasValidGuardPoints())
            {
                int startIndex = GetClosestGuardPointIndex();
                if (startIndex < 0) startIndex = 0;
                _stateMachine.Initialize(new WatchGuardWalkingState(this, _stateMachine, startIndex));
            }
            else
                base.InitializeStateMachine();

            _guardLight?.SetLightActive(false);
        }

        protected override void InitializeComponents()
        {
            base.InitializeComponents();
            if (_guardLight == null)
                _guardLight = GetComponentInChildren<WatchGuardLight>();
        }

        protected override void ResetEnemy()
        {
            if (movement != null) movement.Stop();
            if (animationController != null) animationController.ResetToIdle();
            if (captureHandler != null && captureHandler.IsHoldingPlayer)
                captureHandler.ReleasePlayer();

            CancelSoundRotation();
            ResetDetectionAfterRespawn();
            SetLanternVisible(true);

            if (_stateMachine == null)
                return;

            int closestIndex = GetClosestGuardPointIndex();
            if (closestIndex >= 0)
            {
                TeleportToGuardPoint(closestIndex);
                _stateMachine.ChangeState(new WatchGuardWatchingState(this, _stateMachine, closestIndex));
            }
            else
                _stateMachine.ChangeState(new EnemyIdleState(this, _stateMachine));
        }

        public bool HasValidGuardPoints()
        {
            if (guardPoints == null || guardPoints.Length == 0)
                return false;

            foreach (Transform point in guardPoints)
            {
                if (point != null)
                    return true;
            }

            return false;
        }

        public Vector3 GetGuardPointPosition(int index)
        {
            if (!IsValidGuardIndex(index))
                return transform.position;

            return guardPoints[index].position;
        }

        public Transform GetLookTargetForGuardPoint(int index)
        {
            if (lookTargets != null && index >= 0 && index < lookTargets.Length && lookTargets[index] != null)
                return lookTargets[index];

            if (IsValidGuardIndex(index))
                return guardPoints[index];

            return null;
        }

        public int GetNextGuardIndex(int currentIndex)
        {
            if (!HasValidGuardPoints())
                return -1;

            int total = guardPoints.Length;
            for (int offset = 1; offset <= total; offset++)
            {
                int candidate = loopGuardPoints
                    ? (currentIndex + offset) % total
                    : currentIndex + offset;

                if (candidate >= total)
                    return -1;

                if (IsValidGuardIndex(candidate))
                    return candidate;
            }

            return -1;
        }

        public bool HasReachedGuardPoint()
        {
            return movement != null && movement.HasReachedDestination(arrivalThreshold);
        }

        public int GetClosestGuardPointIndex()
        {
            if (!HasValidGuardPoints())
                return -1;

            int closestIndex = -1;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < guardPoints.Length; i++)
            {
                if (guardPoints[i] == null) continue;

                float distance = Vector3.Distance(transform.position, guardPoints[i].position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }

        void TeleportToGuardPoint(int guardIndex)
        {
            if (!IsValidGuardIndex(guardIndex))
                return;

            Vector3 targetPos = guardPoints[guardIndex].position;
            if (movement != null)
                movement.TeleportTo(targetPos);
            else
                transform.position = targetPos;

            Transform lookTarget = GetLookTargetForGuardPoint(guardIndex);
            if (lookTarget != null)
            {
                Vector3 lookDirection = lookTarget.position - targetPos;
                lookDirection.y = 0f;

                if (lookDirection.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.LookRotation(lookDirection.normalized);
            }
        }

        bool IsValidGuardIndex(int index)
        {
            return guardPoints != null && index >= 0 && index < guardPoints.Length && guardPoints[index] != null;
        }
    }
}